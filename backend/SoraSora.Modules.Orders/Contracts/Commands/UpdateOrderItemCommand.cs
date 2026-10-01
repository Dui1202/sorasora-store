namespace SoraSora.Modules.Orders.Contracts.Commands;

public sealed record UpdateOrderItemCommand
{
    public string? ProductName { get; init; }
    public string? Sku { get; init; }
    public string? Id { get; init; }
    public string? ProductVariantId { get; init; }
    public int SellingPrice { get; init; }
    public int Quantity { get; init; }
}
