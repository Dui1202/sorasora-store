namespace SoraSora.Modules.Products.Contracts.Commands.Create
{
    public sealed record CreateProductVariantOptionCommand
    {
        public required string VariationClientId { get; init; }
        public required string VariationOptionClientId { get; init; }
    }
}
