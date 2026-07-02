using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(CreateProductRequest command);
        Task<ProductDto> UpdateProductAsync(UpdateProductRequest command);
        Task<ProductDto?> GetProductByIdAsync(string id);
    }
}
