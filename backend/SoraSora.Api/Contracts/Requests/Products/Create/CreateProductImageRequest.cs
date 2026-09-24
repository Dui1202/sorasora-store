namespace SoraSora.Api.Contracts.Requests.Products.Create;

public sealed record CreateProductImageRequest
{
    public required string ClientId { get; init; }
    public string? Alt { get; init; }
    public required int Position { get; init; }
}
