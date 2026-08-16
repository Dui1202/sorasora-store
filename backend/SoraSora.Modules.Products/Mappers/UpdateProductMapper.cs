using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Requests.Create;
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
        var variations = request.Variations.Select(ToUpdateVariationCommand).ToList();

        return new UpdateProductCommand
        {
            Id = request.Id,
            Version = request.Version,
            Name = request.Name,
            Description = request.Description,
            SupplierCountry = request.SupplierCountry,
            SellMode = request.SellMode,
            IsPublished = request.IsPublished,
            NewProductVariants = newProductVariants,
            RetainedProductVariants = existingProductVariants,
            NewImages = newImages,
            RetainedImages = existingImages,
            Variations = variations,
        };
    }

    private static RetainedProductImageCommand ToExistingProductImageCommand(
        RetainedProductImageRequest request
    )
    {
        return new RetainedProductImageCommand(
            MediaAssetId: request.MediaAssetId,
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
            ProductVariantOptions = request
                .ProductVariantOptions.Select(ToProductVariantOptionCommand)
                .ToList(),
        };
    }

    private static UpdateVariationCommand ToUpdateVariationCommand(
        UpdateVariationRequest request
    )
    {
        return new UpdateVariationCommand
        {
            Id = request.Id,
            ClientId = request.ClientId,
            Name = request.Name,
            Position = request.Position,
            VariationOptions = request.VariationOptions.Select(ToUpdateVariationOptionCommand).ToList(),
        };
    }

    private static UpdateVariationOptionCommand ToUpdateVariationOptionCommand(
        UpdateVariationOptionRequest request
    )
    {
        return new UpdateVariationOptionCommand
        {
            Id = request.Id,
            ClientId = request.ClientId,
            Value = request.Value,
            Position = request.Position,
        };
    }

    private static CreateProductVariantOptionCommand ToProductVariantOptionCommand(
        CreateProductVariantOptionRequest request
    )
    {
        return new CreateProductVariantOptionCommand
        {
            VariationClientId = request.VariationClientId,
            VariationOptionClientId = request.VariationOptionClientId,
        };
    }
}
