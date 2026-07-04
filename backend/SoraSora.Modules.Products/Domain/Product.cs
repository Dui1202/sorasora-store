using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Product : IAuditable
    {
        public string Id { get; private set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // public SupplierCountry SupplierCountry { get; set; }
        // public SellMode SellMode { get; set; }
        // public DateTime? PreorderReleaseAt { get; set; }
        // public ICollection<ProductVariant> ProductVariants { get; set; } = [];
        // public ICollection<Variation> Variations { get; set; } = [];
        public bool IsPublished { get; set; }
        // public bool HasVariation { get; set; }
        // public bool IsArchived { get; set; }
    }
}
