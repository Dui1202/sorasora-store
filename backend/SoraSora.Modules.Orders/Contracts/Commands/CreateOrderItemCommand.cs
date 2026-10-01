namespace SoraSora.Modules.Orders.Contracts.Commands;

public sealed record CreateOrderItemCommand
{
    public required string ProductName { get; init; }
    public string? Sku { get; init; }
    public required string ProductVariantId { get; init; }
    public int SellingPrice { get; init; }
    public int Quantity { get; init; }
}
