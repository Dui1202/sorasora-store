namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariantImage
    {
        public required string Id { get; set; }
        public required string ProductVariantId { get; set; }
        public required string Path { get; set; }
        public string? Alt { get; set; }
        public required ProductVariant ProductVariant { get; set; }
    }
}
