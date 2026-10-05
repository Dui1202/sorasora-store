namespace SoraSora.Api.Contracts.Commands.OrderWorkflow;

public sealed record PlaceOrderWorkflowCommand
{
    public required string WarehouseId { get; init; }
    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public required string CustomerPhoneNumber { get; init; }
    public required string ShippingAddress { get; init; }
    public string? CustomerNote { get; init; }
    public IReadOnlyCollection<PlaceOrderItemCommand> OrderItems { get; init; } = [];
}
