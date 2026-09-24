namespace SoraSora.Api.Contracts.Requests.Products.Create
{
    public sealed record CreateVariationOptionRequest
    {
        public required string ClientId { get; init; }
        public required string Value { get; init; }
        public int Position { get; init; }
    };
}
