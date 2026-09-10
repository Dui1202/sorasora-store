using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Mappers;

internal static class ProductDtoMapper
{
    internal static ProductVariantResponse ToProductVariantResponse(ProductVariant productVariant)
    {
        return new ProductVariantResponse
        {
            Id = productVariant.Id,
            ProductId = productVariant.ProductId,
            Name = productVariant.Name,
            Sku = productVariant.Sku,
            CostPrice = productVariant.CostPrice,
            SellingPrice = productVariant.SellingPrice,
            Position = productVariant.Position,
            IsArchived = productVariant.IsArchived,
            CreatedAt = productVariant.CreatedAt,
            UpdatedAt = productVariant.UpdatedAt,
        };
    }

    internal static ProductResponse ToProductResponse(Product product)
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
        };
    }
}
