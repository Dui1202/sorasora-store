using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Mappers;

public static class ProductDtoMapper
{
    public static ProductVariantResponse ToProductVariantResponse(ProductVariant productVariant)
    {
        return new ProductVariantResponse
        {
            Id = productVariant.Id,
            ClientId = productVariant.ClientId,
            ProductId = productVariant.ProductId,
            Name = productVariant.Name,
            Sku = productVariant.Sku,
            CostPrice = productVariant.CostPrice,
            SellingPrice = productVariant.SellingPrice,
            Position = productVariant.Position,
            IsArchived = productVariant.IsArchived,
            CreatedAt = productVariant.CreatedAt,
            UpdatedAt = productVariant.UpdatedAt,
            ProductVariantImage = productVariant.ProductVariantImage is { } image
                ? ToProductVariantImageResponse(image)
                : null,
            ProductVariantOptions = productVariant
                .ProductVariantOptions.OrderBy(option => option.VariationOptionId)
                .Select(ToProductVariantOptionResponse)
                .ToList(),
        };
    }

    public static ProductResponse ToProductResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Version = product.Version,
            Name = product.Name,
            Description = product.Description,
            IsPublished = product.IsPublished,
            IsArchived = product.IsArchived,
            SupplierCountry = product.SupplierCountry,
            SellMode = product.SellMode,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            ProductImages = product
                .ProductImages.OrderBy(image => image.Position)
                .ThenBy(image => image.Id)
                .Select(ToProductImageDto)
                .ToList(),
            ProductVariants = product
                .ProductVariants.OrderBy(variant => variant.Position)
                .ThenBy(variant => variant.Id)
                .Select(ToProductVariantResponse)
                .ToList(),
            Variations = product
                .Variations.OrderBy(variation => variation.Position)
                .ThenBy(variation => variation.Id)
                .Select(ToVariationResponse)
                .ToList(),
        };
    }

    private static ProductImageDto ToProductImageDto(ProductImage image)
    {
        return new ProductImageDto
        {
            Id = image.Id,
            ProductId = image.ProductId,
            MediaAssetId = image.MediaAssetId,
            Position = image.Position,
        };
    }

    private static ProductVariantImageResponse ToProductVariantImageResponse(
        ProductVariantImage image
    )
    {
        return new ProductVariantImageResponse
        {
            Id = image.Id,
            ProductVariantId = image.ProductVariantId,
            MediaAssetId = image.MediaAssetId,
        };
    }

    private static ProductVariantOptionResponse ToProductVariantOptionResponse(
        ProductVariantOption option
    )
    {
        return new ProductVariantOptionResponse
        {
            ProductVariantId = option.ProductVariantId,
            VariationOptionId = option.VariationOptionId,
        };
    }

    private static VariationResponse ToVariationResponse(Variation variation)
    {
        return new VariationResponse
        {
            Id = variation.Id,
            ProductId = variation.ProductId,
            Name = variation.Name,
            Position = variation.Position,
            CreatedAt = variation.CreatedAt,
            UpdatedAt = variation.UpdatedAt,
            VariationOptions = variation
                .VariationOptions.OrderBy(option => option.Position)
                .ThenBy(option => option.Id)
                .Select(ToVariationOptionResponse)
                .ToList(),
        };
    }

    private static VariationOptionResponse ToVariationOptionResponse(VariationOption option)
    {
        return new VariationOptionResponse
        {
            Id = option.Id,
            VariationId = option.VariationId,
            Value = option.Value,
            Position = option.Position,
            CreatedAt = option.CreatedAt,
            UpdatedAt = option.UpdatedAt,
        };
    }
}
