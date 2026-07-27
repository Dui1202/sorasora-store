namespace SoraSora.Modules.Products.Contracts.Requests.Create;

public sealed record CreateProductVariantSelectedOptionRequest
{
    public required string VariationId { get; init; }
    public required string OptionId { get; init; }
}
