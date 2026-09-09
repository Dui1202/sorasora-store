namespace SoraSora.Modules.Orders.Contracts.Requests;

public sealed record CreateOrderItemRequest
{
    public required string InventoryItemId { get; init; }
    public int SellingPrice { get; init; }
    public int Quantity { get; init; }
}
