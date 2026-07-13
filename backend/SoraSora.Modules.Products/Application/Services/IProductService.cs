using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken
        );
        Task<ProductResponse> UpdateProductAsync(UpdateProductRequest command);
        Task<ProductResponse?> GetProductByIdAsync(string id);
        Task<List<ProductResponse>?> GetAllProductsAsync();
    }
}
