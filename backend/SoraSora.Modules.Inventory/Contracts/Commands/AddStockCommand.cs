namespace SoraSora.Modules.Inventory.Contracts.Commands;

public sealed record AddStockCommand(
    string ProductVariantId,
    int Quantity
);
