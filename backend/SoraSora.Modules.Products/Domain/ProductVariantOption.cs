namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariantOption
    {
        public required string ProductVariantId { get; set; }
        public required string VariationId { get; set; }
        public required string VariationOptionId { get; set; }
        public required ProductVariant ProductVariant { get; set; }
        public required Variation Variation { get; set; }
        public required VariationOption VariationOption { get; set; }
    }
}
