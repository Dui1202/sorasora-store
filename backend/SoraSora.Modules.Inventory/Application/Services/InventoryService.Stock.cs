using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public Task<StockResponse> ReceiveStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.Receive(amount, note),
            cancellationToken
        );

    public Task<StockResponse> ReserveStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.Reserve(amount, note),
            cancellationToken
        );

    public Task<StockResponse> ReleaseStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.Release(amount, note),
            cancellationToken
        );

    public Task<StockResponse> SaleStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.Sale(amount, note),
            cancellationToken
        );

    public Task<StockResponse> ReturnStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.Return(amount, note),
            cancellationToken
        );

    public Task<StockResponse> AdjustOnHandedStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.AdjustOnHanded(amount, note),
            cancellationToken,
            allowNegativeQuantity: true
        );

    public Task<StockResponse> AdjustReservedStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    ) =>
        UpdateStockAsync(
            productVariantId,
            warehouseId,
            quantity,
            description,
            static (stock, amount, note) => stock.AdjustReserved(amount, note),
            cancellationToken,
            allowNegativeQuantity: true
        );

    private async Task<StockResponse> UpdateStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description,
        Action<Stock, int, string?> updateStock,
        CancellationToken cancellationToken,
        bool allowNegativeQuantity = false
    )
    {
        if (!allowNegativeQuantity && quantity < 0)
        {
            throw new InventoryDomainException(
                "Stock quantity cannot be negative.",
                nameof(quantity)
            );
        }

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken
            );

            var inventoryItem =
                await _dbContext
                    .InventoryItems()
                    .Include(x => x.Stocks)
                    .SingleOrDefaultAsync(
                        x => x.ProductVariantId == productVariantId,
                        cancellationToken
                    )
                ?? throw new NotFoundException("The inventory item is not found");

            var stock =
                inventoryItem.Stocks.SingleOrDefault(x => x.WarehouseId == warehouseId)
                ?? throw new NotFoundException("Stock in the specified warehouse is not found.");

            // Use the transaction's snapshot even if this context already tracked the stock.
            await _dbContext.Entry(stock).ReloadAsync(cancellationToken);
            if (_dbContext.Entry(stock).State == EntityState.Detached)
            {
                throw new NotFoundException("Stock in the specified warehouse is not found.");
            }

            updateStock(stock, quantity, description);

            await _dbContext.SaveChangesAsync(cancellationToken);
            var response = InventoryItemDtoMapper.ToStockResponse(stock);
            await transaction.CommitAsync(cancellationToken);

            return response;
        }
        catch (Exception ex)
            when (ex.GetBaseException()
                    is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }
            )
        {
            throw new ConflictException(
                "Stock changed concurrently. Retry the operation.",
                InventoryErrorCodes.StockUpdateConflict,
                ex
            );
        }
    }
}
