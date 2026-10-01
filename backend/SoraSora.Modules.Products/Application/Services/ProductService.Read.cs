using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
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
        var products = await ProductQuery()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => ProductDtoMapper.ToProductResponse(x))
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<ProductResponse> GetProductByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var product =
            await ProductQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
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
                .Where(x => !x.IsArchived)
                .Include(x => x.ProductVariantImage)
                .Include(x => x.ProductVariantOptions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product variant {id} was not found.");

        return ProductDtoMapper.ToProductVariantResponse(productVariant);
    }

    public async Task<Product> GetProductByIdForUpdateAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        return await ProductQuery()
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ProductNotFoundException(id);
    }

    private IQueryable<Product> ProductQuery()
    {
        return _dbContext
            .Products()
            .Where(x => !x.IsArchived)
            .AsSplitQuery()
            .Include(x => x.ProductImages)
            .Include(x => x.ProductVariants.Where(x => !x.IsArchived))
                .ThenInclude(x => x.ProductVariantImage)
            .Include(x => x.ProductVariants.Where(x => !x.IsArchived))
                .ThenInclude(x => x.ProductVariantOptions)
            .Include(x => x.Variations)
                .ThenInclude(x => x.VariationOptions);
    }
}
