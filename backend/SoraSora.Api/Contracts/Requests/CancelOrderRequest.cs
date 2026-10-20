public sealed record CancelOrderRequest
{
    public required string OrderId { get; init; }
    public required string WarehouseId { get; init; }
    public Guid Version { get; init; }
}
