using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Infrastructure.Storage;
using SoraSora.Modules.Products.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task<ProductResponse> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var uploadedImagePaths = new List<string>();
        var imagePathsToRemove = new List<string>();
        var product =
            await _dbContext
                .Products()
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Product {command.Id} was not found.");

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
                    throw new BadRequestException(
                        $"Product Variant {productVariant.Id} doesn't belong to Product {product.Id}."
                    );
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
                var path = ProductImagePathBuilder.BuildProductVariantImagePath(
                    productId: product.Id,
                    productVariantId: productVariantToUpdate.Id,
                    fileName: newImage.File.FileName
                );

                uploadedImagePaths.Add(path);
                await _fileStorageService.UploadFileAsync(newImage.File, path, cancellationToken);

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

                var path = ProductImagePathBuilder.BuildProductVariantImagePath(
                    productId: product.Id,
                    productVariantId: productVariant.Id,
                    fileName: image.File.FileName
                );

                uploadedImagePaths.Add(path);
                await _fileStorageService.UploadFileAsync(image.File, path, cancellationToken);

                productVariant.SetProductVariantImage(ProductVariantImage.Create(path, image.Alt));
            }

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

            //Add new product images
            foreach (var image in command.NewImages)
            {
                var file = image.File;
                var path = ProductImagePathBuilder.BuildProductImagePath(product.Id, file.FileName);

                uploadedImagePaths.Add(path);
                await _fileStorageService.UploadFileAsync(file, path, cancellationToken);

                var productImage = ProductImage.Create(path, image.Alt, image.Position);
                product.AddImage(productImage);
            }

            //Update product images
            foreach (var image in command.RetainedImages)
            {
                if (!existingProductImagesById.TryGetValue(image.Path, out var productImage))
                {
                    throw new BadRequestException(
                        $"Image with path {image.Path} does not belong to {product.Id}"
                    );
                }

                productImage.UpdateDetails(image.Alt, image.Position);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var path in uploadedImagePaths)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(path, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.ProductImageCleanupFailed(ex, path);
                }
            }

            throw;
        }

        foreach (var path in imagePathsToRemove)
        {
            await TryDeleteFileAsync(path);
        }

        return ProductDtoMapper.ToProductResponse(product);
    }

    private async Task TryDeleteFileAsync(string path)
    {
        try
        {
            await _fileStorageService.DeleteFileAsync(path, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.ProductImageDeleteFailed(ex, path);
        }
    }
}
