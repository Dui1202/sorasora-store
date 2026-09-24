namespace SoraSora.Modules.Inventory.Contracts.Commands
{
    public sealed record CreateInventoryItemCommand(
        string ProductVariantId,
        string? Sku,
        int InitialQuantity
    );
}
