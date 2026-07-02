using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Application
{
    public sealed class ProductService : IProductService
    {
        private readonly SoraSoraDbContext _dbContext;

        public ProductService(SoraSoraDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductRequest command)
        {
            var validator = new CreateProductRequestValidator();
            var result = validator.Validate(command);

            if (!result.IsValid)
            {
                throw new Exception(
                    "Validation failed: "
                        + string.Join(", ", result.Errors.Select(e => e.ErrorMessage))
                );
            }

            var product = new Product
            {
                Name = command.Name,
                Description = command.Description,
                SupplierCountry = command.SupplierCountry,
                SellMode = command.SellMode,
                PreorderReleaseAt = command.PreorderReleaseAt,
                IsPublished = command.IsPublished,
                HasVariation = command.HasVariation,
                IsArchived = false,
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                IsPublished = product.IsPublished,
            };
        }

        public Task<ProductDto?> GetProductByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ProductDto> UpdateProductAsync(UpdateProductRequest command)
        {
            throw new NotImplementedException();
        }
    }
}
