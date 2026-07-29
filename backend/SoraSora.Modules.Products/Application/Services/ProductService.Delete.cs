using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Products.Application.Exceptions;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task DeleteProductAsync(
        string id,
        Guid version,
        CancellationToken cancellationToken
    )
    {
        var product =
            await _dbContext
                .Products()
                .Where(x => !x.IsArchived)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        if (product.Version != version)
        {
            throw new ProductConcurrencyException(product.Id);
        }

        var productImages = product.ProductImages.ToList();
        var productVariantImages = product
            .ProductVariants.Select(x => x.ProductVariantImage)
            .OfType<ProductVariantImage>()
            .ToList();
        var imagePaths = productImages
            .Select(x => x.Path)
            .Concat(productVariantImages.Select(x => x.Path))
            .ToHashSet(StringComparer.Ordinal);

        product.Archive();
        product.AdvanceVersion();

        foreach (var productVariant in product.ProductVariants)
        {
            productVariant.Archive();
        }

        _dbContext.RemoveRange(productImages);
        _dbContext.RemoveRange(productVariantImages);

        foreach (var path in imagePaths)
        {
            _dbContext.StorageCleanupTasks().Add(StorageCleanupTask.Create(path));
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ProductConcurrencyException(product.Id, ex);
        }

        _logger.ProductArchived(product.Name, product.Id);
    }
}
