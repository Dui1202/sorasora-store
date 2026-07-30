namespace SoraSora.Modules.Products.Contracts.Requests.Create
{
    public sealed record CreateProductVariantOptionRequest
    {
        public required string VariationOptionClientId { get; init; }
    }
}
