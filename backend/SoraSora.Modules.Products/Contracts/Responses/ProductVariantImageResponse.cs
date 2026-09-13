namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record ProductVariantImageResponse
{
    public required string Id { get; init; }
    public required string ProductVariantId { get; init; }
    public required string MediaAssetId { get; init; }
}
