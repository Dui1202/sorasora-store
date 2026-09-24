using SoraSora.Api.Contracts.Requests.Products.Create;

namespace SoraSora.Api.Contracts.Requests.Products.Update;

public sealed record RetainedProductVariantRequest
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public string? Sku { get; init; }

    public int CostPrice { get; init; }

    public int SellingPrice { get; init; }

    public int Position { get; init; }

    public required InventoryUpdateAction Action { get; init; }

    public required int Quantity { get; init; }

    public CreateProductVariantImageRequest? NewImage { get; init; }

    public IReadOnlyCollection<CreateProductVariantOptionRequest> ProductVariantOptions { get; init; } =
    [];
}
