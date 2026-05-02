using NUlid;

namespace SoraSora.Modules.Products.Domain;

public sealed class Product
{
    public String Id { get; private set; } = Ulid.NewUlid().ToString();

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? FranchiseId { get; set; }

    public SupplierCountry SupplierCountry { get; set; }

    public SellMode SellMode { get; set; } = SellMode.IN_STOCK;

    public DateTime? PreorderReleaseAt { get; set; }

    public Franchise? Franchise { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = [];

    public ICollection<Variation> Variations { get; set; } = [];

    public ICollection<ProductImage> ProductImages { get; set; } = [];

    public bool IsPublished { get; set; }

    public bool HasVariation { get; set; }

    public bool IsArchived { get; set; }
}
