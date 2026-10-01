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
        Product product,
        UpdateProductCommand command,
        CancellationToken cancellationToken
    );
    Task ArchiveProductAsync(string id, Guid version, CancellationToken cancellationToken);
    Task<ProductResponse> GetProductByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    );
    Task<List<ProductResponse>?> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<Product> GetProductByIdForUpdateAsync(string id, CancellationToken cancellationToken = default);
    Task<ProductVariantResponse> GetProductVariantByIdAsync(
        string id,
        CancellationToken cancellationToken
    );
}
