using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application.Services;

public interface IProductService
{
    Task<ProductResponse> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken
    );
    Task<ProductResponse> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    );
    Task DeleteProductAsync(string id, Guid version, CancellationToken cancellationToken);
    Task<ProductResponse> GetProductByIdAsync(string id);
    Task<List<ProductResponse>?> GetAllProductsAsync();
}
