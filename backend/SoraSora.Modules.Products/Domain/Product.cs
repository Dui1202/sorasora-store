using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Product : IAuditable
    {
        private readonly List<ProductImage> _productImages = [];
        public IReadOnlyCollection<ProductImage> ProductImages => _productImages;
        public string Id { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public SupplierCountry SupplierCountry { get; set; }

        // public SellMode SellMode { get; set; }
        // public DateTime? PreorderReleaseAt { get; set; }
        // public ICollection<ProductVariant> ProductVariants { get; set; } = [];
        // public ICollection<Variation> Variations { get; set; } = [];
        public bool IsPublished { get; set; }

        // public bool HasVariation { get; set; }
        public bool IsArchived { get; set; } = true;

        public static Product Create(
            string name,
            string? description,
            SupplierCountry supplierCountry,
            bool isPublished
        )
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name is required!", nameof(name));
            }

            return new Product
            {
                Name = name,
                Description = description,
                SupplierCountry = supplierCountry,
                IsPublished = isPublished,
            };
        }

        public void AddImage(string path, string? alt, int position)
        {
            _productImages.Add(ProductImage.Create(path, alt, position));
        }
    }
}
