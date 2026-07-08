using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken
        );
        Task<ProductDto> UpdateProductAsync(UpdateProductRequest command);
        Task<ProductDto?> GetProductByIdAsync(string id);
        Task<List<ProductDto>?> GetAllProductsAsync();
    }
}
