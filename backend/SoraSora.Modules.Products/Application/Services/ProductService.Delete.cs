using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Products.Application.Exceptions;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task DeleteProductAsync(string id, CancellationToken cancellationToken)
    {
        var product =
            await _dbContext
                .Products()
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product {id} was not found.");

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
