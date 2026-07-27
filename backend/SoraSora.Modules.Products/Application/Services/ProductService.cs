using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Infrastructure.Storage;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService(
    SoraSoraDbContext dbContext,
    IFileStorageService fileStorageService,
    ILogger<ProductService> logger
) : IProductService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<ProductService> _logger = logger;

    public Task DeleteProductAsync(string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
}
