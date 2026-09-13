public sealed record CreateInventoryItemCommand(
    string ProductVariantId,
    string? Sku,
    int InitialQuantity
);
