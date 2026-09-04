public sealed record InventoryItemResponse(
    string ProductVariantId,
    IReadOnlyCollection<StockResponse> Stocks
);
