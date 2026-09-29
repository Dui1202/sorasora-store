using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow
{
    public async Task<ProductResponse> UpdateProductAsync(
        UpdateProductWorkflowCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);
        var updateProductCommand = UpdateProductWorkflowMapper.ToUpdateProductCommand(command);
        Product? updatedProduct = null;

        var commitAttempted = false;
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var product =
                await _dbContext
                    .Products()
                    .Where(x => !x.IsArchived)
                    .AsSplitQuery()
                    .Include(x => x.ProductImages)
                    .Include(x => x.ProductVariants)
                        .ThenInclude(x => x.ProductVariantImage)
                    .Include(x => x.ProductVariants)
                        .ThenInclude(x => x.ProductVariantOptions)
                    .Include(x => x.Variations)
                        .ThenInclude(x => x.VariationOptions)
                    .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException($"Product with Id {command.Id} can't be found.");

            await RemoveUnretainedImageFilesAsync(product, command, cancellationToken);

            updatedProduct = await _productService.UpdateProductAsync(
                product,
                updateProductCommand,
                cancellationToken
            );

            await UpdateProductImagePositionsAsync(
                product,
                updateProductCommand,
                cancellationToken
            );

            await UploadAndAttachProductImageFilesAsync(
                updatedProduct,
                command,
                uploadedMediaAssetIds,
                cancellationToken
            );

            await UpdateInventoryItemsAsync(product, command, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Updated product {ProductId} and its inventory",
                updatedProduct.Id
            );

            return ProductDtoMapper.ToProductResponse(updatedProduct);
        }
        catch
        {
            await TryRollbackAndCleanUpAsync(
                updatedProduct,
                uploadedMediaAssetIds,
                commitAttempted
            );
            throw;
        }
        finally
        {
            try
            {
                await transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to dispose the update-product transaction");
            }
        }
    }

    private async Task UpdateInventoryItemsAsync(
        Product product,
        UpdateProductWorkflowCommand command,
        CancellationToken cancellationToken
    )
    {
        var variantsByClientId = product.ProductVariants.ToDictionary(
            x => x.ClientId,
            StringComparer.Ordinal
        );

        foreach (var variantCommand in command.RetainedProductVariants)
        {
            var updateInventoryItemCommand =
                UpdateProductWorkflowMapper.ToUpdateInventoryItemCommand(variantCommand);

            await _inventoryService.UpdateInventoryItemAsync(
                updateInventoryItemCommand,
                cancellationToken
            );
        }

        foreach (var variantCommand in command.NewProductVariants)
        {
            if (!variantsByClientId.TryGetValue(variantCommand.ClientId, out var variant))
            {
                throw new InvalidOperationException();
            }

            var createInventoryItemCommand =
                CreateProductWorkflowMapper.ToCreateInventoryItemCommand(variant, variantCommand);

            await _inventoryService.CreateInventoryItemAsync(
                createInventoryItemCommand,
                cancellationToken
            );
        }
    }

    private async Task UploadAndAttachProductImageFilesAsync(
        Product product,
        UpdateProductWorkflowCommand command,
        ISet<string> uploadedMediaAssetIds,
        CancellationToken cancellationToken
    )
    {
        var productImageUploadJobs = BuildProductImageUploadJobsForUpdateCommand(command, product);

        var imageUploadJobs = productImageUploadJobs.Select(x => x.Job).ToList();

        var mediaUploadResult = await _imageMediaService.UploadImagesAsync(
            imageUploadJobs,
            cancellationToken
        );

        AttachUploadedMedia(productImageUploadJobs, mediaUploadResult, uploadedMediaAssetIds);
    }

    private async Task RemoveUnretainedImageFilesAsync(
        Product product,
        UpdateProductWorkflowCommand command,
        CancellationToken cancellationToken
    )
    {
        var imageAssetIdsToRemove = new HashSet<string>(StringComparer.Ordinal);

        var existingProductImageAssetIds = product
            .ProductImages.Select(x => x.MediaAssetId)
            .ToHashSet(StringComparer.Ordinal);

        var retainedProductImageAssetIds = command
            .RetainedImages.Select(x => x.MediaAssetId)
            .ToHashSet(StringComparer.Ordinal);

        var unretainedProductImagesAssetIds = existingProductImageAssetIds.Except(
            retainedProductImageAssetIds
        );

        var existingVariantByIds = product.ProductVariants.ToDictionary(
            x => x.Id,
            StringComparer.Ordinal
        );
        var existingVariantIds = existingVariantByIds.Keys;

        var retainedVariantIds = command
            .RetainedProductVariants.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var unretainedVariantIds = existingVariantIds.Except(retainedVariantIds);

        //Add unretained product variant's image to the deletion list
        foreach (var id in unretainedVariantIds)
        {
            if (!existingVariantByIds.TryGetValue(id, out var variant))
            {
                throw new InvalidOperationException("Cannot find the unretained variant");
            }

            if (variant.ProductVariantImage is { } existingImage)
            {
                imageAssetIdsToRemove.Add(existingImage.MediaAssetId);
            }
        }

        var retainedVariantByIds = command.RetainedProductVariants.ToDictionary(
            x => x.Id,
            StringComparer.Ordinal
        );

        //Add retained product variant's image that has been replaced to the deletion list
        foreach (var id in retainedVariantIds)
        {
            if (!retainedVariantByIds.TryGetValue(id, out var retainedVariant))
            {
                throw new InvalidOperationException();
            }

            if (retainedVariant.NewImage is not null)
            {
                if (!existingVariantByIds.TryGetValue(id, out var variant))
                {
                    throw new InvalidOperationException();
                }

                if (variant.ProductVariantImage is { } existingImage)
                {
                    imageAssetIdsToRemove.Add(existingImage.MediaAssetId);
                }
            }
        }

        foreach (var assetId in unretainedProductImagesAssetIds.ToList())
        {
            product.RemoveImageByAssetId(assetId);
        }

        imageAssetIdsToRemove.UnionWith(unretainedProductImagesAssetIds);

        await _imageMediaService.ScheduleImageDeletionAsync(
            imageAssetIdsToRemove,
            cancellationToken
        );
    }

    private async Task UpdateProductImagePositionsAsync(
        Product product,
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingProductImagesByAssetId = product.ProductImages.ToDictionary(
            x => x.MediaAssetId,
            StringComparer.Ordinal
        );
        var retainedProductImageAssetIds = command
            .RetainedImages.Select(x => x.MediaAssetId)
            .ToHashSet(StringComparer.Ordinal);

        //Apply temporary positions
        var occupiedPositions = product.ProductImages.Select(x => x.Position).ToList();
        var finalPositions = command.RetainedImages.Select(x => x.Position).ToList();
        var tempPositions = AllocateTemporaryPositions(
            occupiedPositions,
            finalPositions,
            command.RetainedImages.Count
        );
        var tempIndex = 0;

        foreach (var image in command.RetainedImages)
        {
            if (
                !existingProductImagesByAssetId.TryGetValue(
                    image.MediaAssetId,
                    out var productImage
                )
            )
            {
                throw new InvalidOperationException();
            }

            productImage.UpdateDetails(position: tempPositions[tempIndex++]);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        //Update product images
        var altTextByMediaAssetId = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var image in command.RetainedImages)
        {
            if (
                !existingProductImagesByAssetId.TryGetValue(
                    image.MediaAssetId,
                    out var productImage
                )
            )
            {
                throw new InvalidOperationException();
            }

            productImage.UpdateDetails(position: image.Position);
            altTextByMediaAssetId.Add(image.MediaAssetId, image.Alt);
        }

        await _imageMediaService.UpdateImageAltTextsAsync(altTextByMediaAssetId, cancellationToken);
    }

    private static List<int> AllocateTemporaryPositions(
        IEnumerable<int> occupiedPositions,
        IEnumerable<int> finalPositions,
        int count
    )
    {
        var unavailablePositions = occupiedPositions.Concat(finalPositions).ToHashSet();

        var temporaryPositions = new List<int>(count);

        for (var candidate = int.MaxValue; temporaryPositions.Count < count; candidate--)
        {
            if (unavailablePositions.Add(candidate))
            {
                temporaryPositions.Add(candidate);
            }
        }

        return temporaryPositions;
    }
}
