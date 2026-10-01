using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Contracts.Responds;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Modules.Inventory.Exceptions;
using SoraSora.Modules.Inventory.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public Task<StockResponse> SetOnHandedStockAsync(
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
            static (stock, target, note) =>
                stock.AdjustOnHanded(target - stock.OnHandedQuantity, note),
            cancellationToken
        );

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
            var stock =
                await _dbContext
                    .Set<Stock>()
                    .FromSqlInterpolated(
                        $"""
                        SELECT s.*
                        FROM "Stocks" AS s
                        INNER JOIN "InventoryItems" AS i
                            ON i."Id" = s."InventoryItemId"
                        WHERE i."ProductVariantId" = {productVariantId}
                          AND s."WarehouseId" = {warehouseId}
                        FOR UPDATE OF s
                        """
                    )
                    .AsTracking()
                    .SingleOrDefaultAsync(cancellationToken)
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

            return response;
        }
        catch (Exception ex)
            when (ex.GetBaseException()
                    is PostgresException { SqlState: PostgresErrorCodes.DeadlockDetected }
            )
        {
            throw new ConflictException(
                "Stock update could not be completed because of a concurrent operation.",
                InventoryErrorCodes.StockUpdateConflict,
                ex
            );
        }
    }
}
