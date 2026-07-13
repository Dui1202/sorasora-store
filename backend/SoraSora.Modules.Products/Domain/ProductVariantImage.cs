using NUlid;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariantImage
    {
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string ProductVariantId { get; private set; } = null!;
        public string Path { get; private set; } = null!;
        public string? Alt { get; private set; }
        public ProductVariant ProductVariant { get; private set; } = null!;
    }
}
