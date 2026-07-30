namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariantOption
    {
        public string ProductVariantId { get; private set; } = null!;
        public string VariationOptionId { get; private set; } = null!;
        public ProductVariant ProductVariant { get; private set; } = null!;
        public VariationOption VariationOption { get; private set; } = null!;
    }
}
