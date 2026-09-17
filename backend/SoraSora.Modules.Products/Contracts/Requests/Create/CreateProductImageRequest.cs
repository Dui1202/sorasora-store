namespace SoraSora.Modules.Products.Contracts.Requests.Create;

public sealed record CreateProductImageRequest
{
    public required string ClientId { get; init; }
    public string? Alt { get; init; }
    public required int Position { get; init; }
}
