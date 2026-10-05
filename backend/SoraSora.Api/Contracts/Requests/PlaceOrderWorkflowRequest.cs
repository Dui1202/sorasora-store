using SoraSora.Modules.Orders.Contracts.Requests;

namespace SoraSora.Api.Contracts.Requests;

public sealed record PlaceOrderWorkflowRequest
{
    public required string WarehouseId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public required string CustomerPhoneNumber { get; init; }
    public required string ShippingAddress { get; init; }
    public string? CustomerNote { get; init; }
    public IReadOnlyCollection<CreateOrderItemRequest> OrderItems { get; init; } = [];
}
