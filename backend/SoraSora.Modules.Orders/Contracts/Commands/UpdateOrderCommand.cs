namespace SoraSora.Modules.Orders.Contracts.Commands;

public sealed record UpdateOrderCommand
{
    public string? Id { get; init; }

    public required string CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public required string CustomerPhoneNumber { get; init; }
    public required string ShippingAddress { get; init; }
    public string? CustomerNote { get; init; }
    public string? StoreNote { get; init; }
    public IReadOnlyCollection<UpdateOrderItemCommand> OrderItems { get; init; } = [];
}
