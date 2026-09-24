using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public async Task<InventoryItem> CreateInventoryItemAsync(
        CreateInventoryItemCommand command,
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
            await _dbContext.Warehouses().SingleOrDefaultAsync(x => x.IsDefault, cancellationToken)
            ?? throw new InvalidOperationException("The default warehouse is missing");

        var defaultStock = Stock.Create(defaultWarehouse.Id, command.InitialQuantity);

        var inventoryItem = InventoryItem.Create(command.ProductVariantId, command.Sku);
        inventoryItem.AddStock(defaultStock);

        _dbContext.InventoryItems().Add(inventoryItem);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException
                    is PostgresException
                    {
                        SqlState: PostgresErrorCodes.UniqueViolation,
                        ConstraintName: "UX_InventoryItems_ProductVariantId"
                    }
            )
        {
            throw new ConflictException(
                "Inventory item for this product variant has already existed."
            );
        }

        return inventoryItem;
    }
}
