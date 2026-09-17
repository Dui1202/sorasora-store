namespace SoraSora.Modules.Products.Contracts.Requests.Create;

public sealed record CreateProductVariantImageRequest
{
    public required string ClientId { get; init; }
    public string? Alt { get; init; }
}
