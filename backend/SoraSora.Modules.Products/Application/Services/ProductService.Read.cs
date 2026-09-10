using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Services;

//TODO: Add pagination
public sealed partial class ProductService
{
    public async Task<List<ProductResponse>?> GetAllProductsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Products()
            .AsNoTracking()
            .Where(x => !x.IsArchived)
            .OrderBy(x => x.Name)
            .Select(product => ProductDtoMapper.ToProductResponse(product))
            .ToListAsync();
    }

    public async Task<ProductResponse> GetProductByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var product =
            await _dbContext
                .Products()
                .AsNoTracking()
                .Where(x => !x.IsArchived)
                .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new ProductNotFoundException(id);

        return ProductDtoMapper.ToProductResponse(product);
    }

    public async Task<ProductVariantResponse> GetProductVariantByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var productVariant =
            await _dbContext
                .ProductVariants()
                .AsNoTracking()
                .Where(x => !x.IsArchived && !x.Product.IsArchived)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product variant {id} was not found.");

        return ProductDtoMapper.ToProductVariantResponse(productVariant);
    }
}
