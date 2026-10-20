using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Contracts.Commands;

public sealed record CreateOrderItemCommand
{
    public required string ProductName { get; init; }
    public required string ProductVariantName { get; init; }
    public string? Sku { get; init; }
    public required string ProductVariantId { get; init; }
    public int SellingPrice { get; init; }
    public int Quantity { get; init; }
    public OrderSaleType SaleType { get; init; }
    public string? WarehouseId { get; init; }
}
