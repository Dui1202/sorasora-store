using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Requests.Update;

namespace SoraSora.Modules.Products.Mappers;

public static class UpdateProductMapper
{
    public static UpdateProductCommand ToUpdateProductCommand(UpdateProductRequest request)
    {
        var newImages = request
            .NewImages.Select(CreateProductDtoMapper.ToCreateProductImageCommand)
            .ToList();
        var newProductVariants = request
            .NewProductVariants.Select(CreateProductDtoMapper.ToCreateProductVariantCommand)
            .ToList();
        var existingProductVariants = request
            .RetainedProductVariants.Select(ToExistingProductVariantCommand)
            .ToList();
        var existingImages = request.RetainedImages.Select(ToExistingProductImageCommand).ToList();

        return new UpdateProductCommand
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            SupplierCountry = request.SupplierCountry,
            SellMode = request.SellMode,
            IsPublished = request.IsPublished,
            HasVariation = request.HasVariation,
            NewProductVariants = newProductVariants,
            RetainedProductVariants = existingProductVariants,
            NewImages = newImages,
            RetainedImages = existingImages,
        };
    }

    private static RetainedProductImageCommand ToExistingProductImageCommand(
        RetainedProductImageRequest request
    )
    {
        return new RetainedProductImageCommand(
            Path: request.Path,
            Alt: request.Alt,
            Position: request.Position
        );
    }

    private static RetainedProductVariantCommand ToExistingProductVariantCommand(
        RetainedProductVariantRequest request
    )
    {
        var newImage = CreateProductDtoMapper.ToCreateProductVariantImageCommandOrNull(
            request.NewImage
        );

        return new RetainedProductVariantCommand
        {
            Id = request.Id,
            Name = request.Name,
            Sku = request.Sku,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Position = request.Position,
            NewImage = newImage,
        };
    }
}
