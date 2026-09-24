namespace SoraSora.Api.Contracts.Requests.Products.Create
{
    public sealed record CreateProductVariantOptionRequest
    {
        public required string VariationClientId { get; init; }
        public required string VariationOptionClientId { get; init; }
    }
}
