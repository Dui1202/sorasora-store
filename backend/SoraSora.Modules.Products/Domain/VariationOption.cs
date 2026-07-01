using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class VariationOption : IAuditable
    {
        public required string Id { get; set; }
        public required string VariationId { get; set; }
        public required string Value { get; set; }
        public required int Position { get; set; } = 0;
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
        public required Variation Variation { get; set; }
        public required ICollection<ProductVariantOption> ProductVariantOptions { get; set; } = [];
    }
}
