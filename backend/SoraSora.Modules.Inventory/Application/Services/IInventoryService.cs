using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Application.Services;

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryItem>> GetAllInventoryItemsAsync(
        CancellationToken cancellationToken
    );
    Task<InventoryItem> GetInventoryItemByIdAsync(string id, CancellationToken cancellationToken);

    Task<InventoryItem> GetInventoryItemByProductVariantIdAsync(
        string productVariantId,
        CancellationToken cancellationToken = default
    );

    Task<InventoryItem> CreateInventoryItemAsync(
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken
    );

    Task<InventoryItem> UpdateInventoryItemAsync(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken
    );

    Task<StockResponse> ReceiveStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReserveStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReleaseStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> SaleStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReturnStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustOnHandedStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustReservedStockAsync(
        string productVariantId,
        string warehouseId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );
}
