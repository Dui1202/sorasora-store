using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(CreateProductRequest command);
        Task<ProductDto?> GetProductByIdAsync(string id);
    }
}
