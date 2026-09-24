namespace SoraSora.Api.Contracts.Requests.Products.Create;

public sealed record CreateProductVariantRequest
{
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public int CostPrice { get; init; }
    public int SellingPrice { get; init; }
    public int Position { get; init; }
    public int Quantity { get; init; }
    public required CreateProductVariantImageRequest ProductVariantImage { get; init; }
    public IReadOnlyCollection<CreateProductVariantOptionRequest> ProductVariantOptions { get; init; } =
    [];
}
