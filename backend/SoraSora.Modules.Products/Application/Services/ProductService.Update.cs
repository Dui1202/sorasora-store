using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Infrastructure.Storage;
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
        var imagePathsToUpload = new HashSet<string>();
        var imagePathsToRemove = new HashSet<string>();
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

            await UpdateProductVariantsAsync(
                product,
                command,
                imagePathsToUpload,
                imagePathsToRemove,
                cancellationToken
            );

            await UpdateProductImagesAsync(
                product,
                command,
                imagePathsToUpload,
                imagePathsToRemove,
                cancellationToken
            );

            foreach (var path in imagePathsToRemove)
            {
                _dbContext.StorageCleanupTasks().Add(StorageCleanupTask.Create(path));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            // A rollback does not reset EF Core's tracked entity states. Detach the rolled-back
            // product changes so the cleanup save cannot accidentally persist them.
            _dbContext.ChangeTracker.Clear();

            try
            {
                foreach (var path in imagePathsToUpload)
                {
                    _dbContext.StorageCleanupTasks().Add(StorageCleanupTask.Create(path));
                }

                if (imagePathsToUpload.Count > 0)
                {
                    await _dbContext.SaveChangesAsync(CancellationToken.None);
                }
            }
            catch (Exception cleanupException)
            {
                foreach (var path in imagePathsToUpload)
                {
                    _logger.ProductImageCleanupFailed(cleanupException, path);
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

    private async Task UpdateProductVariantsAsync(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imagePathsToUpload,
        HashSet<string> imagePathsToRemove,
        CancellationToken cancellationToken
    )
    {
        var existingProductVariantsById = product.ProductVariants.ToDictionary(
            variant => variant.Id,
            StringComparer.Ordinal
        );
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
                imagePathsToRemove.Add(productVariant.ProductVariantImage.Path);
            }

            product.RemoveProductVariant(productVariant);
        }
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

        //Update retained product variants
        foreach (var productVariant in command.RetainedProductVariants)
        {
            //Update product variant details
            if (
                !existingProductVariantsById.TryGetValue(
                    productVariant.Id,
                    out var productVariantToUpdate
                )
            )
            {
                throw new ProductVariantOwnershipException(productVariant.Id, product.Id);
            }

            productVariantToUpdate.UpdateDetails(
                name: productVariant.Name,
                sku: productVariant.Sku,
                costPrice: productVariant.CostPrice,
                sellingPrice: productVariant.SellingPrice,
                position: productVariant.Position
            );

            //Update product variant image operation
            var newImage = productVariant.NewImage;

            ///Skip if the new image command is null
            if (newImage is null)
            {
                continue;
            }

            ///Delete the original image
            if (productVariantToUpdate.ProductVariantImage is not null)
            {
                imagePathsToRemove.Add(productVariantToUpdate.ProductVariantImage.Path);
            }

            ///Upload the new image
            var path = await ProcessImageAsync(
                newImage.File,
                format =>
                    ProductImagePathBuilder.BuildProductVariantImagePath(
                        productId: product.Id,
                        productVariantId: productVariantToUpdate.Id,
                        format
                    ),
                imagePathsToUpload,
                cancellationToken
            );

            productVariantToUpdate.SetProductVariantImage(
                ProductVariantImage.Create(path, newImage.Alt)
            );
        }

        //Add new product variant
        foreach (var newProductVariant in command.NewProductVariants)
        {
            var productVariant = ProductVariant.Create(
                name: newProductVariant.Name,
                sku: newProductVariant.Sku,
                costPrice: newProductVariant.CostPrice,
                sellingPrice: newProductVariant.SellingPrice,
                position: newProductVariant.Position
            );

            product.AddProductVariant(productVariant);

            ///Add and upload image
            var image = newProductVariant.ProductVariantImage;

            var path = await ProcessImageAsync(
                image.File,
                format =>
                    ProductImagePathBuilder.BuildProductVariantImagePath(
                        productId: product.Id,
                        productVariantId: productVariant.Id,
                        format
                    ),
                imagePathsToUpload,
                cancellationToken
            );

            productVariant.SetProductVariantImage(ProductVariantImage.Create(path, image.Alt));
        }
    }

    private async Task UpdateProductImagesAsync(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imagePathsToUpload,
        HashSet<string> imagePathsToRemove,
        CancellationToken cancellationToken
    )
    {
        var existingProductImagesById = product.ProductImages.ToDictionary(
            x => x.Path,
            StringComparer.Ordinal
        );
        var retainedProductImagePaths = command
            .RetainedImages.Select(x => x.Path)
            .ToHashSet(StringComparer.Ordinal);
        var productImagesToRemove = product
            .ProductImages.Where(x => !retainedProductImagePaths.Contains(x.Path))
            .ToList();
        //Remove non-retained product images
        foreach (var image in productImagesToRemove)
        {
            imagePathsToRemove.Add(image.Path);

            product.RemoveImageByPath(image.Path);
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
            if (!existingProductImagesById.TryGetValue(image.Path, out var productImage))
            {
                throw new ProductImageOwnershipException(image.Path, product.Id);
            }

            productImage.UpdatePosition(tempPositions[tempIndex++]);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        //Update product images
        foreach (var image in command.RetainedImages)
        {
            if (!existingProductImagesById.TryGetValue(image.Path, out var productImage))
            {
                throw new ProductImageOwnershipException(image.Path, product.Id);
            }

            productImage.UpdateDetails(image.Alt, image.Position);
        }

        //Add new product images
        foreach (var image in command.NewImages)
        {
            var file = image.File;
            var path = await ProcessImageAsync(
                file,
                format => ProductImagePathBuilder.BuildProductImagePath(product.Id, format),
                imagePathsToUpload,
                cancellationToken
            );

            var productImage = ProductImage.Create(path, image.Alt, image.Position);
            product.AddImage(productImage);
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
}
