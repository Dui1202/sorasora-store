using Microsoft.EntityFrameworkCore;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public async Task ArchiveInventoryItemAsync(
        string productVariantId,
        CancellationToken cancellationToken
    )
    {
        var inventoryItem =
            await _dbContext
                .InventoryItems()
                .Where(x => !x.IsArchived)
                .SingleOrDefaultAsync(
                    x => x.ProductVariantId == productVariantId,
                    cancellationToken
                )
            ?? throw new NotFoundException(
                $"Inventory item with product variant Id {productVariantId} can't not be found."
            );

        inventoryItem.Archive();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveInventoryItemsAsync(
        ISet<string> productVariantIds,
        CancellationToken cancellationToken = default
    )
    {
        var inventoryItems = await _dbContext
            .InventoryItems()
            .Where(x => !x.IsArchived && productVariantIds.Contains(x.ProductVariantId))
            .ToListAsync(cancellationToken);

        foreach (var item in inventoryItems)
        {
            item.Archive();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
