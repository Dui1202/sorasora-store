namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record ProductImageDto
{
    public required string Id { get; init; }
    public required string ProductId { get; init; }
    public required string MediaAssetId { get; init; }
    public int Position { get; init; }
}
