namespace SoraSora.Modules.Products.Contracts.Requests.Create
{
    public sealed record CreateVariationRequest
    {
        public required string Name { get; init; }
        public int Position { get; init; }
        public required IReadOnlyCollection<CreateVariationOptionRequest> VariationOptions { get; init; }
    }
}
