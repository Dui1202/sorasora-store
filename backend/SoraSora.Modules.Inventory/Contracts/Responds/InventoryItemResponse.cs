namespace SoraSora.Modules.Inventory.Contracts.Responds;

public sealed record InventoryItemResponse(
    string Id,
    string ProductVariantId,
    IReadOnlyCollection<StockResponse> Stocks
);
