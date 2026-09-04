using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Shared.Exceptions;

public sealed partial class InventoryService : IInventoryService
{
    public async Task<InventoryItemResponse> CreateInventoryItemAsync(
        CreateInventoryCommand command,
        CancellationToken cancellationToken
    )
    {
        var hasAlreadyExisted = await _dbContext
            .InventoryItems()
            .AnyAsync(x => x.ProductVariantId == command.ProductVariantId, cancellationToken);

        if (hasAlreadyExisted)
        {
            throw new ConflictException(
                "Inventory item for this product variant has already existed."
            );
        }

        var defaultWarehouse =
            await _dbContext.Warehouses().SingleOrDefaultAsync(x => x.IsDefault)
            ?? throw new InvalidOperationException("The default warehouse is missing");

        var defaultStock = Stock.Create(defaultWarehouse.Id, command.InitialQuantity);

        var inventoryItem = InventoryItem.Create(command.ProductVariantId, command.Sku);
        inventoryItem.AddStock(defaultStock);

        _dbContext.InventoryItems().Add(inventoryItem);

        await _dbContext.SaveChangesAsync();

        return InventoryItemDtoMapper.ToInventoryItemResponse(inventoryItem);
    }
}
