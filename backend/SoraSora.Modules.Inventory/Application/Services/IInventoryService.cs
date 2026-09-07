namespace SoraSora.Modules.Inventory.Application.Services;

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryItemResponse>> GetAllInventoryItemsAsync(
        CancellationToken cancellationToken
    );
    Task<InventoryItemResponse> GetInventoryItemByIdAsync(
        string id,
        CancellationToken cancellationToken
    );
    Task<InventoryItemResponse> CreateInventoryItemAsync(
        CreateInventoryCommand command,
        CancellationToken cancellationToken
    );

    Task<StockResponse> ReceiveStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReserveStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReleaseStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> SaleStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReturnStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustOnHandedStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustReservedStockAsync(
        string inventoryId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );
}
