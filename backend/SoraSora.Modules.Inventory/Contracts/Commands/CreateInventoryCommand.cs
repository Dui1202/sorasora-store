public sealed record CreateInventoryCommand(
    string ProductVariantId,
    string? Sku,
    int InitialQuantity
);
