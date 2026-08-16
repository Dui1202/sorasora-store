using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Requests.Update;

public sealed record RetainedProductVariantRequest
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public string? Sku { get; init; }

    public int CostPrice { get; init; }

    public int SellingPrice { get; init; }

    public int Position { get; init; }

    public CreateProductVariantImageRequest? NewImage { get; init; }

    public IReadOnlyCollection<CreateProductVariantOptionRequest> ProductVariantOptions { get; init; } =
    [];
}
