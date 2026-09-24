namespace SoraSora.Api.Contracts.Requests.Products.Create
{
    public sealed record CreateVariationRequest
    {
        public required string ClientId { get; init; }
        public required string Name { get; init; }
        public int Position { get; init; }
        public required IReadOnlyCollection<CreateVariationOptionRequest> VariationOptions { get; init; }
    }
}
