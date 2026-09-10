namespace SoraSora.Modules.Orders.Contracts.Requests;

public sealed record CreateOrderItemRequest
{
    public required string ProductVariantId { get; init; }
    public int SellingPrice { get; init; }
    public int Quantity { get; init; }
}
