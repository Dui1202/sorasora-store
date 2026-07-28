using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Mappers;

internal static class CreateProductDtoMapper
{
    private const int DefaultPosition = 0;

    internal static CreateProductCommand ToCreateProductCommand(CreateProductRequest request)
    {
        var productImages = request.ProductImages.Select(ToCreateProductImageCommand).ToList();

        var productVariants = request.HasVariation
            ? request.ProductVariants.Select(ToCreateProductVariantCommand).ToList()
            : [ToCreateProductVariantCommand(request)];

        return new CreateProductCommand(
            Name: request.Name,
            Description: request.Description,
            SupplierCountry: request.SupplierCountry,
            SellMode: request.SellMode,
            IsPublished: request.IsPublished,
            HasVariation: request.HasVariation,
            ProductImages: productImages,
            ProductVariants: productVariants
        );
    }

    internal static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
        CreateProductImageRequest request
    )
    {
        return new CreateProductVariantImageCommand(File: request.File, Alt: request.Alt);
    }

    internal static CreateProductImageCommand ToCreateProductImageCommand(
        CreateProductImageRequest request
    )
    {
        return new CreateProductImageCommand(
            File: request.File,
            Alt: request.Alt,
            Position: request.Position
        );
    }

    private static CreateProductVariantCommand ToCreateProductVariantCommand(
        CreateProductRequest request
    )
    {
        var baseImage =
            request.ProductImages.MinBy(image => image.Position)
            ?? throw new InvalidOperationException(
                "A product without variations must contain at least one product image."
            );

        var costPrice =
            request.BaseCostPrice
            ?? throw new InvalidOperationException(
                "A product without variations must have a base cost price."
            );

        var sellingPrice =
            request.BaseSellingPrice
            ?? throw new InvalidOperationException(
                "A product without variations must have a base selling price."
            );

        return new CreateProductVariantCommand(
            Name: request.Name,
            Sku: request.BaseSku,
            CostPrice: costPrice,
            SellingPrice: sellingPrice,
            Position: DefaultPosition,
            ProductVariantImage: ToCreateProductVariantImageCommand(baseImage)
        );
    }

    internal static CreateProductVariantCommand ToCreateProductVariantCommand(
        CreateProductVariantRequest request
    )
    {
        return new CreateProductVariantCommand(
            Name: request.Name,
            Sku: request.Sku,
            CostPrice: request.CostPrice,
            SellingPrice: request.SellingPrice,
            Position: request.Position,
            ProductVariantImage: ToCreateProductVariantImageCommand(request.ProductVariantImage)
        );
    }

    internal static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
        CreateProductVariantImageRequest request
    )
    {
        return new CreateProductVariantImageCommand(File: request.File, Alt: request.Alt);
    }

    internal static CreateProductVariantImageCommand? ToCreateProductVariantImageCommandOrNull(
        CreateProductVariantImageRequest? request
    )
    {
        if (request is null)
        {
            return null;
        }

        return new CreateProductVariantImageCommand(File: request.File, Alt: request.Alt);
    }
}
