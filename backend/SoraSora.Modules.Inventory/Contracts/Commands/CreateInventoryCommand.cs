namespace SoraSora.Modules.Inventory.Contracts.Commands
{
    public sealed record CreateInventoryItemCommand(
        string ProductVariantId,
        int InitialQuantity
    );
}
