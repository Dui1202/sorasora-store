using System.Security;
using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Contracts;
using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Modules.Inventory.Infrastructure.Persistence.Extension;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public partial class InventoryService : IInventoryService
{
    public async Task<InventoryItem> UpdateInventoryItemAsync(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken
    )
    {
        var inventoryItem = await GetInventoryItemByProductVariantIdForUpdateAsync(
            command.ProductVariantId,
            cancellationToken
        );

        var defaultWarehouse =
            await _dbContext.Warehouses().SingleOrDefaultAsync(x => x.IsDefault, cancellationToken)
            ?? throw new InvalidOperationException("The default warehouse is missing");

        var stock =
            inventoryItem.Stocks.SingleOrDefault(x => x.WarehouseId == defaultWarehouse.Id)
            ?? throw new NotFoundException("Stock in the specified warehouse is not found.");

        if (
            command.Action == InventoryUpdateAction.SetTo
            && command.Quantity != stock.OnHandedQuantity
        )
        {
            await SetOnHandedStockAsync(
                command.ProductVariantId,
                defaultWarehouse.Id,
                command.Quantity,
                "Update onhanded quantity during update operation",
                cancellationToken
            );
        }
        else if (command.Action == InventoryUpdateAction.AdjustBy && command.Quantity != 0)
        {
            await AdjustOnHandedStockAsync(
                command.ProductVariantId,
                defaultWarehouse.Id,
                command.Quantity,
                "Update onhanded quantity during update operation",
                cancellationToken
            );
        }

        return inventoryItem;
    }
}
