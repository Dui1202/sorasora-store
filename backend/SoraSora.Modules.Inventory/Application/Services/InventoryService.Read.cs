using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Shared.Exceptions;

public sealed partial class InventoryService : IInventoryService
{
    public async Task<IReadOnlyCollection<InventoryItemResponse>> GetAllInventoryItemsAsync(
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .InventoryItems()
            .Include(x => x.Stocks)
            .AsNoTracking()
            .Select(x => InventoryItemDtoMapper.ToInventoryItemResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItemResponse> GetInventoryItemByIdAsync(
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

        return InventoryItemDtoMapper.ToInventoryItemResponse(inventoryItem);
    }

    public async Task<InventoryItemResponse> GetInventoryItemByProductVariantIdAsync(
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

        return InventoryItemDtoMapper.ToInventoryItemResponse(inventoryItem);
    }
}
