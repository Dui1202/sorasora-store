namespace SoraSora.Api.Contracts.Requests.Products.Update;

public sealed record UpdateVariationRequest
{
    public string? Id { get; init; }
    public required string ClientId { get; init; }
    public required string Name { get; init; }
    public int Position { get; init; }
    public IReadOnlyCollection<UpdateVariationOptionRequest> VariationOptions { get; init; } = [];
}
