public sealed record InventoryItemResponse(
    string Id,
    string ProductVariantId,
    IReadOnlyCollection<StockResponse> Stocks
);
