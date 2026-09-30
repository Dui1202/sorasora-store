using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task ArchiveProductAsync(
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
        var imageAssetIds = productImages
            .Select(image => image.MediaAssetId)
            .Concat(productVariantImages.Select(image => image.MediaAssetId))
            .ToHashSet(StringComparer.Ordinal);

        product.Archive();
        product.AdvanceVersion();

        _dbContext.RemoveRange(productImages);
        _dbContext.RemoveRange(productVariantImages);

        await _imageMediaService.ScheduleImageDeletionAsync(imageAssetIds, cancellationToken);

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
