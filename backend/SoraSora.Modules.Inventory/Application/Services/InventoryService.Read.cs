using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Modules.Inventory.Infrastructure.Persistence.Extension;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public async Task<IReadOnlyCollection<InventoryItem>> GetAllInventoryItemsAsync(
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .InventoryItems()
            .Include(x => x.Stocks)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem> GetInventoryItemByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var inventoryItem =
            await _dbContext
                .InventoryItems()
                .Include(x => x.Stocks)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("The inventory item is not found");

        return inventoryItem;
    }

    public async Task<InventoryItem> GetInventoryItemByProductVariantIdAsync(
        string productVariantId,
        CancellationToken cancellationToken = default
    )
    {
        var inventoryItem =
            await _dbContext
                .InventoryItems()
                .Include(x => x.Stocks)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId, cancellationToken)
            ?? throw new NotFoundException("The inventory item is not found");

        return inventoryItem;
    }
}
