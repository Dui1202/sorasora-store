using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class Product : IAuditable
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 2000;

    private readonly List<ProductImage> _productImages = [];
    private readonly List<ProductVariant> _productVariants = [];
    public IReadOnlyCollection<ProductImage> ProductImages => _productImages;
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    public SupplierCountry SupplierCountry { get; private set; }
    public SellMode SellMode { get; private set; }

    // public DateTime? PreorderReleaseAt { get; set; }
    public IReadOnlyCollection<ProductVariant> ProductVariants => _productVariants;

    // public ICollection<Variation> Variations { get; set; } = [];
    public bool IsPublished { get; private set; }
    public bool HasVariation { get; private set; }
    public bool IsArchived { get; private set; }

    private Product() { }

    public static Product Create(
        string name,
        string? description,
        SupplierCountry supplierCountry,
        SellMode sellMode,
        bool isPublished,
        bool hasVariation
    )
    {
        ValidateDetails(name, description, supplierCountry, sellMode);

        return new Product
        {
            Name = name,
            Description = description,
            SupplierCountry = supplierCountry,
            SellMode = sellMode,
            IsPublished = isPublished,
            HasVariation = hasVariation,
            IsArchived = false,
        };
    }

    public void UpdateDetails(
        string name,
        string? description,
        SupplierCountry supplierCountry,
        SellMode sellMode,
        bool isPublished,
        bool hasVariation
    )
    {
        ValidateDetails(name, description, supplierCountry, sellMode);

        Name = name;
        Description = description;
        SupplierCountry = supplierCountry;
        SellMode = sellMode;
        IsPublished = isPublished;
        HasVariation = hasVariation;
    }

    public void AddImage(string path, string? alt, int position)
    {
        _productImages.Add(ProductImage.Create(path, alt, position));
    }

    public void AddImage(ProductImage productImage)
    {
        _productImages.Add(productImage);
    }

    public void AddProductVariant(ProductVariant productVariant)
    {
        _productVariants.Add(productVariant);
    }

    public void RemoveImageByPath(string path)
    {
        _productImages.RemoveAll(image => image.Path == path);
    }

    public void RemoveProductVariant(string id)
    {
        _productVariants.RemoveAll(variant => variant.Id == id);
    }

    public void RemoveProductVariant(ProductVariant productVariant)
    {
        _productVariants.Remove(productVariant);
    }

    public void AdvanceVersion()
    {
        Version = Guid.NewGuid();
    }

    private static void ValidateDetails(
        string name,
        string? description,
        SupplierCountry supplierCountry,
        SellMode sellMode
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required!", nameof(name));
        }

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Product name cannot exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        if (description?.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Product description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description)
            );
        }

        if (!Enum.IsDefined(supplierCountry))
        {
            throw new ArgumentOutOfRangeException(nameof(supplierCountry));
        }

        if (!Enum.IsDefined(sellMode))
        {
            throw new ArgumentOutOfRangeException(nameof(sellMode));
        }
    }
}
