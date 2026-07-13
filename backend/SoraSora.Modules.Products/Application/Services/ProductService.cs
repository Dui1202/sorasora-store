using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Infrastructure.Storage;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services
{
    public sealed class ProductService(
        SoraSoraDbContext dbContext,
        IFileStorageService fileStorageService,
        ILogger<ProductService> logger
    ) : IProductService
    {
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<ProductService> _logger = logger;

        public async Task<ProductResponse> CreateProductAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken
        )
        {
            var uploadedProductImagePaths = new List<string>();

            try
            {
                var product = Product.Create(
                    name: command.Name,
                    description: command.Description,
                    supplierCountry: command.SupplierCountry,
                    isPublished: command.IsPublished
                );

                foreach (var imageRequest in command.ProductImages)
                {
                    var file = imageRequest.File;
                    var path = ProductImagePathBuilder.BuildProductImagePath(
                        productId: product.Id,
                        fileName: file.FileName
                    );
                    var alt = imageRequest.Alt;
                    var position = imageRequest.Position;

                    uploadedProductImagePaths.Add(path);
                    await _fileStorageService.UploadFileAsync(file, path, cancellationToken);

                    product.AddImage(path, alt, position);
                }

                _dbContext.Products().Add(product);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Created product {ProductName} - {ProductId}!",
                    product.Name,
                    product.Id
                );

                return ProductDtoMapper.ToProductResponse(product);
            }
            catch
            {
                foreach (var path in uploadedProductImagePaths)
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(path, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Failed to delete uploaded product image {ImagePath} during cleanup",
                            path
                        );
                    }
                }

                throw;
            }
        }

        public async Task<List<ProductResponse>?> GetAllProductsAsync()
        {
            return await _dbContext
                .Products()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(product => ProductDtoMapper.ToProductResponse(product))
                .ToListAsync();
        }

        public async Task<ProductResponse?> GetProductByIdAsync(string id)
        {
            var product = await _dbContext
                .Products()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null)
            {
                return null;
            }

            return ProductDtoMapper.ToProductResponse(product);
        }

        public Task<ProductResponse> UpdateProductAsync(UpdateProductRequest command)
        {
            throw new NotImplementedException();
        }
    }
}
