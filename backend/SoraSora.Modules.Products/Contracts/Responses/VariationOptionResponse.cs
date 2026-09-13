namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record VariationOptionResponse
{
    public required string Id { get; init; }
    public required string VariationId { get; init; }
    public required string Value { get; init; }
    public int Position { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
