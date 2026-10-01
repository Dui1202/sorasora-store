namespace SoraSora.Modules.Orders.Contracts.Requests;

public sealed record CreateOrderRequest
{
    public required string WarehouseId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public required string CustomerPhoneNumber { get; init; }
    public required string ShippingAddress { get; init; }
    public string? CustomerNote { get; init; }
    public string? StoreNote { get; init; }
    public IReadOnlyCollection<CreateOrderItemRequest> OrderItems { get; init; } = [];
}
