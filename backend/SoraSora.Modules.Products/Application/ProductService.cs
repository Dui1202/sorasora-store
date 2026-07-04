using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

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
                IsPublished = command.IsPublished,
            };

            _dbContext.Products().Add(product);
            await _dbContext.SaveChangesAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                IsPublished = product.IsPublished,
            };
        }

        public async Task<List<ProductDto>?> GetAllProductsAsync()
        {
            return await _dbContext
                .Products()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    IsPublished = product.IsPublished,
                })
                .ToListAsync();
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            var product = await _dbContext
                .Products()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null)
            {
                return null;
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                IsPublished = true,
            };
        }

        public Task<ProductDto> UpdateProductAsync(UpdateProductRequest command)
        {
            throw new NotImplementedException();
        }
    }
}
