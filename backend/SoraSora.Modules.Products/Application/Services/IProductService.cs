using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Application.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken
    );
    Task<Product> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    );
    Task DeleteProductAsync(string id, Guid version, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<Product>?> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductVariantResponse> GetProductVariantByIdAsync(
        string id,
        CancellationToken cancellationToken
    );
}
