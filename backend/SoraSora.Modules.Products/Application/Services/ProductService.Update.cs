using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

//TODO: Archive product variants instead of deleting them permanently
public sealed partial class ProductService
{
    public async Task<ProductResponse> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);
        var imageAssetIdsToRemove = new HashSet<string>(StringComparer.Ordinal);
        var product =
            await _dbContext
                .Products()
                .Where(x => !x.IsArchived)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new ProductNotFoundException(command.Id);

        if (product.Version != command.Version)
        {
            throw new ProductConcurrencyException(product.Id);
        }

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            try
            {
                product.UpdateDetails(
                    command.Name,
                    command.Description,
                    command.SupplierCountry,
                    command.SellMode,
                    command.IsPublished,
                    command.HasVariation
                );
                product.AdvanceVersion();

                var existingProductVariantsById = product.ProductVariants.ToDictionary(
                    variant => variant.Id,
                    StringComparer.Ordinal
                );

                RemoveProductVariantsFromProduct(product, command, imageAssetIdsToRemove);

                await UpdateProductVariantPositions(
                    product,
                    command,
                    existingProductVariantsById,
                    cancellationToken
                );

                UpdateProductVariants(
                    product,
                    command,
                    existingProductVariantsById,
                    imageAssetIdsToRemove,
                    out var updatedProductVariants,
                    out var createdProductVariants
                );

                await UpdateProductImagePositionsAsync(
                    product,
                    command,
                    imageAssetIdsToRemove,
                    cancellationToken
                );

                var productImageUploadJobs = BuildProductImageUploadJobForUpdateCommand(
                    product,
                    command,
                    updatedProductVariants,
                    createdProductVariants
                );

                var mediaUploadResults = await _imageMediaService.UploadImagesAsync(
                    productImageUploadJobs.Select(productJob => productJob.Job).ToList(),
                    cancellationToken
                );

                AttachUploadedMedia(
                    productImageUploadJobs,
                    mediaUploadResults,
                    uploadedMediaAssetIds
                );

                await _imageMediaService.DeleteImagesAsync(
                    imageAssetIdsToRemove,
                    cancellationToken
                );

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
        catch (Exception ex)
        {
            // A rollback does not reset EF Core's tracked entity states. Detach the rolled-back
            // product changes so the cleanup save cannot accidentally persist them.
            _dbContext.ChangeTracker.Clear();

            try
            {
                if (uploadedMediaAssetIds.Count > 0)
                {
                    await _imageMediaService.DeleteImagesAsync(
                        uploadedMediaAssetIds,
                        CancellationToken.None
                    );

                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
            }
            catch (Exception cleanupException)
            {
                foreach (var mediaAssetId in uploadedMediaAssetIds)
                {
                    _logger.ProductImageCleanupFailed(cleanupException, mediaAssetId);
                }
            }

            if (ex is DbUpdateConcurrencyException)
            {
                throw new ProductConcurrencyException(product.Id, ex);
            }

            if (ex is ProductDomainException domainException)
            {
                throw new ProductValidationException(domainException.Message, domainException);
            }

            throw;
        }

        _logger.ProductUpdated(product.Name, product.Id);

        return ProductDtoMapper.ToProductResponse(product);
    }

    private void RemoveProductVariantsFromProduct(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imageAssetIdsToRemove
    )
    {
        var retainedProductVariantsId = command
            .RetainedProductVariants.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var productVariantsToRemove = product
            .ProductVariants.Where(x => !retainedProductVariantsId.Contains(x.Id))
            .ToList();

        //Remove non-retained product variant
        foreach (var productVariant in productVariantsToRemove)
        {
            if (productVariant.ProductVariantImage is not null)
            {
                imageAssetIdsToRemove.Add(productVariant.ProductVariantImage.MediaAssetId);
            }

            product.RemoveProductVariant(productVariant);
        }
    }

    private async Task UpdateProductVariantPositions(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        CancellationToken cancellationToken
    )
    {
        var occupiedPositions = product.ProductVariants.Select(x => x.Position).ToList();
        var finalPositions = command.RetainedProductVariants.Select(x => x.Position).ToList();
        var tempPositions = AllocateTemporaryPositions(
            occupiedPositions,
            finalPositions,
            command.RetainedProductVariants.Count
        );
        var tempIndex = 0;
        foreach (var productVariant in command.RetainedProductVariants)
        {
            if (
                !existingProductVariantsById.TryGetValue(
                    productVariant.Id,
                    out var productVariantToUpdate
                )
            )
            {
                throw new ProductVariantOwnershipException(productVariant.Id, product.Id);
            }

            productVariantToUpdate.UpdatePosition(tempPositions[tempIndex++]);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void UpdateProductVariants(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        HashSet<string> imageAssetIdsToRemove,
        out List<ImageUpdatedProductVariant> imageUpdatedProductVariants,
        out List<CreatedProductVariant> createdProductVariants
    )
    {
        imageUpdatedProductVariants = [];
        createdProductVariants = [];
        //Update retained product variants
        foreach (var productVariantCommand in command.RetainedProductVariants)
        {
            //Update product variant details
            if (
                !existingProductVariantsById.TryGetValue(
                    productVariantCommand.Id,
                    out var productVariantToUpdate
                )
            )
            {
                throw new ProductVariantOwnershipException(productVariantCommand.Id, product.Id);
            }

            productVariantToUpdate.UpdateDetails(
                name: productVariantCommand.Name,
                sku: productVariantCommand.Sku,
                costPrice: productVariantCommand.CostPrice,
                sellingPrice: productVariantCommand.SellingPrice,
                position: productVariantCommand.Position
            );

            //Update product variant image operation
            var newImage = productVariantCommand.NewImage;

            ///Skip if the new image command is null
            if (newImage is null)
            {
                continue;
            }

            ///Delete the original image
            if (productVariantToUpdate.ProductVariantImage is not null)
            {
                imageAssetIdsToRemove.Add(productVariantToUpdate.ProductVariantImage.MediaAssetId);
            }
            imageUpdatedProductVariants.Add(
                new ImageUpdatedProductVariant(
                    ProductVariant: productVariantToUpdate,
                    Command: productVariantCommand
                )
            );
        }

        //Add new product variant
        foreach (var newProductVariantCommand in command.NewProductVariants)
        {
            var productVariant = ProductVariant.Create(
                name: newProductVariantCommand.Name,
                sku: newProductVariantCommand.Sku,
                costPrice: newProductVariantCommand.CostPrice,
                sellingPrice: newProductVariantCommand.SellingPrice,
                position: newProductVariantCommand.Position
            );

            product.AddProductVariant(productVariant);
            createdProductVariants.Add(
                new CreatedProductVariant(
                    ProductVariant: productVariant,
                    Command: newProductVariantCommand
                )
            );
        }
    }

    private async Task UpdateProductImagePositionsAsync(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imageAssetIdsToRemove,
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

        var productImagesToRemove = product
            .ProductImages.Where(x => !retainedProductImageAssetIds.Contains(x.MediaAssetId))
            .ToList();
        //Remove non-retained product images
        foreach (var image in productImagesToRemove)
        {
            imageAssetIdsToRemove.Add(image.MediaAssetId);

            product.RemoveImageByAssetId(image.MediaAssetId);
        }

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

    private static void AttachUploadedMedia(
        IReadOnlyList<ProductImageUploadJob> productImageUploadJobs,
        IReadOnlyList<MediaUploadResult> mediaUploadResults,
        ISet<string> uploadedMediaAssetIds
    )
    {
        var attachMediaByUploadIds = productImageUploadJobs.ToDictionary(
            productJob => productJob.Job.Id,
            productJob => productJob.Attach,
            StringComparer.Ordinal
        );

        foreach (var result in mediaUploadResults)
        {
            uploadedMediaAssetIds.Add(result.AssetId);
        }

        if (mediaUploadResults.Count != productImageUploadJobs.Count)
        {
            throw new InvalidOperationException(
                "The media service did not return a result for every upload job"
            );
        }

        foreach (var result in mediaUploadResults)
        {
            if (!attachMediaByUploadIds.TryGetValue(result.JobId, out var attach))
            {
                throw new InvalidOperationException(
                    "This media upload result is not from the original upload job"
                );
            }

            attach(result.AssetId);
        }
    }
}
