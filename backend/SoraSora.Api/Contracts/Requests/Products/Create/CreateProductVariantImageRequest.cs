namespace SoraSora.Api.Contracts.Requests.Products.Create;

public sealed record CreateProductVariantImageRequest
{
    public required string ClientId { get; init; }
    public string? Alt { get; init; }
}
