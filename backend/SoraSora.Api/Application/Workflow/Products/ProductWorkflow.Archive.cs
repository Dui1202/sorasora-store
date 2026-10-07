using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow
{
    public async Task ArchiveProductAsync(
        string productId,
        Guid version,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );
        try
        {
            await _productService.ArchiveProductAsync(productId, version, cancellationToken);
            var product = await _productService.GetArchivedProductByIdAsync(
                productId,
                cancellationToken
            );

            var productVariantIds = product
                .ProductVariants.Select(x => x.Id)
                .ToHashSet(StringComparer.Ordinal);

            await _inventoryService.ArchiveInventoryItemsAsync(
                productVariantIds,
                cancellationToken
            );
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);

            throw;
        }
    }
}
