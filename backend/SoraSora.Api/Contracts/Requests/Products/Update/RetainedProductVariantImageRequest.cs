namespace SoraSora.Api.Contracts.Requests.Products.Update;

public sealed record RetainedProductVariantImageRequest
{
    public required string MediaAssetId { get; init; }

    public string? Alt { get; init; }
}
