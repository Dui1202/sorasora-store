using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Api.Contracts.Requests;

public sealed record CreateOrderItemRequest
{
    public required string ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public OrderSaleType SaleType { get; init; }
}
