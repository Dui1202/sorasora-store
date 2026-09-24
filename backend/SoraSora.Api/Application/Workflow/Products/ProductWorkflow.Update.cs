using Microsoft.CodeAnalysis.CSharp.Syntax;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
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
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await _productService.GetProductByIdAsync(command.Id);

            await RemoveUnretainedProductImagesFilesAsync(product, command, cancellationToken);

            updatedProduct = await _productService.UpdateProductAsync(
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

    private async Task RemoveUnretainedProductImagesFilesAsync(
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

        foreach (var id in unretainedVariantIds)
        {
            if (!existingVariantByIds.TryGetValue(id, out var variant))
            {
                throw new InvalidOperationException("Cannot find the unretained variant");
            }

            imageAssetIdsToRemove.Add(variant.ProductVariantImage!.MediaAssetId);
        }

        var retainedVariantByIds = command.RetainedProductVariants.ToDictionary(
            x => x.Id,
            StringComparer.Ordinal
        );

        foreach (var id in retainedVariantIds)
        {
            if (!retainedVariantByIds.TryGetValue(id, out var retainedVariant))
            {
                throw new InvalidOperationException();
            }

            if (retainedVariant.RetainedImage is null)
            {
                if (!existingVariantByIds.TryGetValue(id, out var variant))
                {
                    throw new InvalidOperationException();
                }

                imageAssetIdsToRemove.Add(variant.ProductVariantImage!.MediaAssetId);
            }
        }

        imageAssetIdsToRemove
            .Concat(unretainedProductImagesAssetIds)
            .ToHashSet(StringComparer.Ordinal);

        await _imageMediaService.ScheduleImageDeletionAsync(
            imageAssetIdsToRemove,
            cancellationToken
        );
    }
}
