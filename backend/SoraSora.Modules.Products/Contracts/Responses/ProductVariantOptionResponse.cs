namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record ProductVariantOptionResponse
{
    public required string ProductVariantId { get; init; }
    public required string VariationOptionId { get; init; }
}
