namespace SoraSora.Api.Contracts.Requests.Products.Update;

public sealed record UpdateVariationOptionRequest
{
    public string? Id { get; init; }
    public required string ClientId { get; init; }
    public required string Value { get; init; }
    public int Position { get; init; }
}
