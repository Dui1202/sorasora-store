using NUlid;
using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Shared.Helper;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Product : IAuditable
    {
        private readonly List<ProductImage> _productImages = [];
        public IReadOnlyCollection<ProductImage> ProductImages => _productImages;
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string Name { get; private set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public SupplierCountry SupplierCountry { get; set; }
        public SellMode SellMode { get; set; }

        // public DateTime? PreorderReleaseAt { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; } = [];

        // public ICollection<Variation> Variations { get; set; } = [];
        public bool IsPublished { get; set; }
        public bool HasVariation { get; set; }
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

        public static Product Create(CreateProductCommand command)
        {
            return new Product
            {
                Name = command.Name,
                Description = command.Description,
                SupplierCountry = command.SupplierCountry,
                IsPublished = command.IsPublished,
            };
        }

        public void AddImage(string path, string? alt, int position)
        {
            _productImages.Add(ProductImage.Create(path, alt, position));
        }

        public void AddImage(ProductImage productImage)
        {
            _productImages.Add(productImage);
        }
    }
}
