namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record VariationResponse
{
    public required string Id { get; init; }
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public int Position { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<VariationOptionResponse> VariationOptions { get; init; } = [];
}
