using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Mappers
{
    internal static class ProductDtoMapper
    {
        internal static ProductResponse ToProductResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                IsPublished = product.IsPublished,
                IsArchived = product.IsArchived,
                SupplierCountry = product.SupplierCountry,
            };
        }
    }
}
