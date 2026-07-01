using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Responses;

namespace SoraSora.Modules.Products.Application
{
    public sealed class ProductService : IProductService
    {
        public private readonly Task<ProductDto> CreateProductAsync(CreateProductRequest command)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDto?> GetProductByIdAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
