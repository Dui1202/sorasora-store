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

    Task<InventoryItemResponse> GetInventoryItemByProductVariantIdAsync(
        string productVariantId,
        CancellationToken cancellationToken = default
    );

    Task<InventoryItemResponse> CreateInventoryItemAsync(
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken
    );

    Task<StockResponse> ReceiveStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReserveStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReleaseStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> SaleStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> ReturnStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustOnHandedStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );

    Task<StockResponse> AdjustReservedStockAsync(
        string productVariantId,
        int quantity,
        string? description = null,
        CancellationToken cancellationToken = default
    );
}
