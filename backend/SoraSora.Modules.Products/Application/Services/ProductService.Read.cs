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
    public async Task<List<Product>?> GetAllProductsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var products = await _dbContext
            .Products()
            .AsNoTracking()
            .Where(x => !x.IsArchived)
            .AsSplitQuery()
            .Include(x => x.ProductImages)
            .Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductVariantImage)
            .Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductVariantOptions)
            .Include(x => x.Variations)
                .ThenInclude(x => x.VariationOptions)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<Product> GetProductByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var product =
            await _dbContext
                .Products()
                .AsNoTracking()
                .Where(x => !x.IsArchived)
                .AsSplitQuery()
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantOptions)
                .Include(x => x.Variations)
                    .ThenInclude(x => x.VariationOptions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        return product;
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
                .Include(x => x.ProductVariantImage)
                .Include(x => x.ProductVariantOptions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product variant {id} was not found.");

        return ProductDtoMapper.ToProductVariantResponse(productVariant);
    }
}
