using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Variation : IAuditable
    {
        public required string Id { get; set; } = null!;

        public required string ProductId { get; set; }

        public required Product Product { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public required int Position { get; set; }
        public required ICollection<VariationOption> VariationOptions { get; set; } = [];
        public required ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = [];
    }
}
