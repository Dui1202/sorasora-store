using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Infrastructure.Storage;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task<ProductResponse> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var uploadedImagePaths = new List<string>();
        var product = Product.Create(
            name: command.Name,
            description: command.Description,
            supplierCountry: command.SupplierCountry,
            sellMode: command.SellMode,
            isPublished: command.IsPublished,
            hasVariation: command.HasVariation
        );

        try
        {
            foreach (var productVariantCommand in command.ProductVariants)
            {
                var productVariant = ProductVariant.Create(
                    name: productVariantCommand.Name,
                    sku: productVariantCommand.Sku,
                    costPrice: productVariantCommand.CostPrice,
                    sellingPrice: productVariantCommand.SellingPrice,
                    position: productVariantCommand.Position
                );

                var file = productVariantCommand.ProductVariantImage.File;
                var alt = productVariantCommand.ProductVariantImage.Alt;
                var path = ProductImagePathBuilder.BuildProductVariantImagePath(
                    productId: product.Id,
                    productVariantId: productVariant.Id,
                    fileName: file.FileName
                );

                uploadedImagePaths.Add(path);
                await _fileStorageService.UploadFileAsync(file, path, cancellationToken);

                productVariant.SetProductVariantImage(ProductVariantImage.Create(path, alt));

                product.AddProductVariant(productVariant);
            }

            foreach (var imageRequest in command.ProductImages)
            {
                var file = imageRequest.File;
                var path = ProductImagePathBuilder.BuildProductImagePath(
                    productId: product.Id,
                    fileName: file.FileName
                );
                var alt = imageRequest.Alt;
                var position = imageRequest.Position;

                uploadedImagePaths.Add(path);
                await _fileStorageService.UploadFileAsync(file, path, cancellationToken);

                product.AddImage(path, alt, position);
            }
            _dbContext.Products().Add(product);
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

        _logger.ProductCreated(product.Name, product.Id);

        return ProductDtoMapper.ToProductResponse(product);
    }
}
