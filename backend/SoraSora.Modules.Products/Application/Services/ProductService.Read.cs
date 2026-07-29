using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task<List<ProductResponse>?> GetAllProductsAsync()
    {
        return await _dbContext
            .Products()
            .AsNoTracking()
            .Where(x => !x.IsArchived)
            .OrderBy(x => x.Name)
            .Select(product => ProductDtoMapper.ToProductResponse(product))
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetProductByIdAsync(string id)
    {
        var product = await _dbContext
            .Products()
            .AsNoTracking()
            .Where(x => !x.IsArchived)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
        {
            return null;
        }

        return ProductDtoMapper.ToProductResponse(product);
    }
}
