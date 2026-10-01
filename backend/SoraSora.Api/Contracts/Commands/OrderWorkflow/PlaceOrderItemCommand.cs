namespace SoraSora.Api.Contracts.Commands.OrderWorkflow;

public sealed record PlaceOrderItemCommand
{
    public required string ProductVariantId { get; init; }
    public int Quantity { get; init; }
}
