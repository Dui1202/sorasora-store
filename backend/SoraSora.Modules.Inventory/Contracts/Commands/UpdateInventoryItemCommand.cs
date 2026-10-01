namespace SoraSora.Modules.Inventory.Contracts.Commands;

public sealed record UpdateInventoryItemCommand()
{
    public required string ProductVariantId { get; init; }
    public InventoryUpdateAction Action { get; init; }
    public int Quantity { get; init; }
}
