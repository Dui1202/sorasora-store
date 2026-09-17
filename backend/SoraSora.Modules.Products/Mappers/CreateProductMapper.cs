using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Mappers;

public static class CreateProductDtoMapper
{
    private const int DefaultPosition = 0;

    public static CreateProductCommand ToCreateProductCommand(
        CreateProductRequest request,
        IDictionary<string, IFormFile> productImageFileByClientId,
        IDictionary<string, IFormFile> variantImageFileByClientId
    )
    {
        var productImages = request
            .ProductImages.Select(x => ToCreateProductImageCommand(x, productImageFileByClientId))
            .ToList();

        var productVariants =
            request.Variations.Count > 0
                ? request
                    .ProductVariants.Select(x =>
                        ToCreateProductVariantCommand(x, variantImageFileByClientId)
                    )
                    .ToList()
                : [ToCreateProductVariantCommand(request, productImageFileByClientId)];

        return new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            SupplierCountry = request.SupplierCountry,
            SellMode = request.SellMode,
            IsPublished = request.IsPublished,
            ProductImages = productImages,
            ProductVariants = productVariants,
            Variations = request.Variations.Select(ToCreateVariationCommand).ToList(),
        };
    }

    // From ProductVariantImageRequest to CreateProductVariantImageCommand
    internal static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
        CreateProductVariantImageRequest request,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        if (!variantImageFilesByClientId.TryGetValue(request.ClientId, out var file))
        {
            throw new InvalidOperationException(
                "Cannot get the product variant image file with its corresponding client Id"
            );
        }

        return new CreateProductVariantImageCommand(File: file, Alt: request.Alt);
    }

    internal static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
        CreateProductImageRequest request,
        IDictionary<string, IFormFile> productImageFilesByClientId
    )
    {
        if (!productImageFilesByClientId.TryGetValue(request.ClientId, out var file))
        {
            throw new InvalidOperationException(
                "Cannot get the product variant image file with its corresponding client Id"
            );
        }

        return new CreateProductVariantImageCommand(File: file, Alt: request.Alt);
    }

    internal static CreateProductImageCommand ToCreateProductImageCommand(
        CreateProductImageRequest request,
        IDictionary<string, IFormFile> productImages
    )
    {
        if (!productImages.TryGetValue(request.ClientId, out var file))
        {
            throw new InvalidOperationException(
                "Cannot get the product image file with its corresponding client Id"
            );
        }

        return new CreateProductImageCommand(
            File: file,
            Alt: request.Alt,
            Position: request.Position
        );
    }

    private static CreateVariationCommand ToCreateVariationCommand(CreateVariationRequest request)
    {
        return new CreateVariationCommand
        {
            ClientId = request.ClientId,
            Name = request.Name,
            Position = request.Position,
            VariationOptions = request
                .VariationOptions.Select(ToCreateVariationOptionCommand)
                .ToList(),
        };
    }

    private static CreateVariationOptionCommand ToCreateVariationOptionCommand(
        CreateVariationOptionRequest request
    )
    {
        return new CreateVariationOptionCommand
        {
            ClientId = request.ClientId,
            Value = request.Value,
            Position = request.Position,
        };
    }

    private static CreateProductVariantCommand ToCreateProductVariantCommand(
        CreateProductRequest request,
        IDictionary<string, IFormFile> productImageFilesByClientId
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

        return new CreateProductVariantCommand
        {
            Name = request.Name,
            Sku = request.BaseSku,
            CostPrice = costPrice,
            SellingPrice = sellingPrice,
            Position = DefaultPosition,
            ProductVariantImage = ToCreateProductVariantImageCommand(
                baseImage,
                productImageFilesByClientId
            ),
        };
    }

    internal static CreateProductVariantCommand ToCreateProductVariantCommand(
        CreateProductVariantRequest request,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new CreateProductVariantCommand
        {
            Name = request.Name,
            Sku = request.Sku,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Position = request.Position,
            ProductVariantImage = ToCreateProductVariantImageCommand(
                request.ProductVariantImage,
                variantImageFilesByClientId
            ),
            ProductVariantOptions = request
                .ProductVariantOptions.Select(ToCreateProductVariantOptionCommand)
                .ToList(),
        };
    }

    internal static CreateProductVariantImageCommand? ToCreateProductVariantImageCommandOrNull(
        CreateProductVariantImageRequest? request,
        IFormFile file
    )
    {
        if (request is null)
        {
            return null;
        }

        return new CreateProductVariantImageCommand(File: file, Alt: request.Alt);
    }

    private static CreateProductVariantOptionCommand ToCreateProductVariantOptionCommand(
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
