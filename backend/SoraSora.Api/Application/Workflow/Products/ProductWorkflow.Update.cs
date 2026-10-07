using Microsoft.EntityFrameworkCore;
using SoraSora.Api.Contracts.Commands.ProductWorkflow;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Mappers;

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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        try
        {
            var product = await _productService.GetProductByIdForUpdateAsync(
                command.Id,
                cancellationToken
            );

            var retainedProductVariantsId = command
                .RetainedProductVariants.Select(x => x.Id)
                .ToHashSet(StringComparer.Ordinal);

            var productVariantsToRemoveId = product
                .ProductVariants.Where(x => !retainedProductVariantsId.Contains(x.Id))
                .Select(x => x.Id)
                .ToHashSet(StringComparer.Ordinal);

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

            await UpdateRetainedImagesAltAsync(product, updateProductCommand, cancellationToken);

            await UploadAndAttachProductImageFilesAsync(
                updatedProduct,
                command,
                uploadedMediaAssetIds,
                cancellationToken
            );

            await UpdateInventoryItemsAsync(
                product,
                command,
                productVariantsToRemoveId,
                cancellationToken
            );

            await _dbContext.SaveChangesAsync(cancellationToken);

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
    }

    private async Task UpdateInventoryItemsAsync(
        Product product,
        UpdateProductWorkflowCommand command,
        ISet<string> productVariantsToRemoveId,
        CancellationToken cancellationToken
    )
    {
        //Update retained inventory items
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

        //Create new inventory items
        foreach (var variantCommand in command.NewProductVariants)
        {
            if (!variantsByClientId.TryGetValue(variantCommand.ClientId, out var variant))
            {
                throw new InvalidOperationException(
                    $"Cannot create inventory for product {product.Id}: the product service did not create a variant with client ID {variantCommand.ClientId}."
                );
            }

            var createInventoryItemCommand =
                CreateProductWorkflowMapper.ToCreateInventoryItemCommand(variant, variantCommand);

            await _inventoryService.CreateInventoryItemAsync(
                createInventoryItemCommand,
                cancellationToken
            );
        }

        //Archive unretained inventory items
        await _inventoryService.ArchiveInventoryItemsAsync(
            productVariantsToRemoveId,
            cancellationToken
        );
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
                throw new InvalidOperationException(
                    $"Cannot schedule image deletion for product {product.Id}: unretained variant {id} is missing from the existing variants."
                );
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
                throw new InvalidOperationException(
                    $"Cannot process image replacement for product {product.Id}: retained variant {id} is missing from the update command."
                );
            }

            if (retainedVariant.NewImage is not null)
            {
                if (!existingVariantByIds.TryGetValue(id, out var variant))
                {
                    throw new ProductVariantOwnershipException(id, product.Id);
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
                throw new ProductImageOwnershipException(image.MediaAssetId, product.Id);
            }

            productImage.UpdateDetails(position: tempPositions[tempIndex++]);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        //Update product images
        foreach (var image in command.RetainedImages)
        {
            if (
                !existingProductImagesByAssetId.TryGetValue(
                    image.MediaAssetId,
                    out var productImage
                )
            )
            {
                throw new ProductImageOwnershipException(image.MediaAssetId, product.Id);
            }

            productImage.UpdateDetails(position: image.Position);
        }
    }

    private async Task UpdateRetainedImagesAltAsync(
        Product product,
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var variantsById = product.ProductVariants.ToDictionary(
            variant => variant.Id,
            StringComparer.Ordinal
        );

        foreach (var variantCommand in command.RetainedProductVariants)
        {
            if (variantCommand.RetainedImage is not { } retainedImage)
            {
                continue;
            }

            if (!variantsById.TryGetValue(variantCommand.Id, out var variant))
            {
                throw new ProductVariantOwnershipException(variantCommand.Id, product.Id);
            }

            if (
                variant.ProductVariantImage is not { } existingImage
                || !string.Equals(
                    existingImage.MediaAssetId,
                    retainedImage.MediaAssetId,
                    StringComparison.Ordinal
                )
            )
            {
                throw new ProductVariantImageOwnershipException(
                    retainedImage.MediaAssetId,
                    variant.Id
                );
            }
        }

        var retainedProductImageAltTextByMediaAssetId = command.RetainedImages.ToDictionary(
            x => x.MediaAssetId,
            x => x.Alt,
            StringComparer.Ordinal
        );

        var retainedVariantImageAltTextByMediaAssetId = command
            .RetainedProductVariants.Where(x => x.RetainedImage is not null)
            .Select(x => x.RetainedImage!)
            .ToDictionary(x => x.MediaAssetId, x => x.Alt);

        var altTextByMediaAssetId = retainedProductImageAltTextByMediaAssetId
            .Concat(retainedVariantImageAltTextByMediaAssetId)
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

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
