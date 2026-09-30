using NUlid;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductVariant : IAuditable
{
    public const int MaxNameLength = 100;
    private readonly List<ProductVariantOption> _productVariantOptions = [];
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ClientId { get; private set; } = null!;
    public string ProductId { get; private set; } = null!;
    public string? Sku { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string Name { get; private set; } = null!;
    public int CostPrice { get; private set; }
    public int SellingPrice { get; private set; }
    public Product Product { get; private set; } = null!;
    public int? Position { get; private set; }
    public bool IsArchived { get; private set; }
    public ProductVariantImage? ProductVariantImage { get; private set; }
    public IReadOnlyCollection<ProductVariantOption> ProductVariantOptions =>
        _productVariantOptions;

    // public required bool IsFullPaymentAllowed { get; set; } = true;
    // public required bool IsDepositRequired { get; set; } = false;
    // public int? DepositPrice { get; set; } = 0;
    private ProductVariant() { }

    public static ProductVariant Create(
        string name,
        string? sku,
        int costPrice,
        int sellingPrice,
        int position,
        string clientId 
    )
    {
        ValidateDetails(name, sku, costPrice, sellingPrice, position);

        return new ProductVariant
        {
            ClientId = clientId,
            Name = name,
            Sku = sku,
            CostPrice = costPrice,
            SellingPrice = sellingPrice,
            Position = position,
            IsArchived = false,
        };
    }

    public void SetProductVariantImage(ProductVariantImage image)
    {
        ProductVariantImage = image;
    }

    public void UpdateDetails(
        string name,
        string? sku,
        int costPrice,
        int sellingPrice,
        int position
    )
    {
        EnsureActive();
        ValidateDetails(name, sku, costPrice, sellingPrice, position);

        Name = name;
        Sku = sku;
        CostPrice = costPrice;
        SellingPrice = sellingPrice;
        Position = position;
    }

    public void UpdatePosition(int position)
    {
        EnsureActive();
        ValidatePosition(position);
        Position = position;
    }

    public void Archive()
    {
        IsArchived = true;
        Position = null;
    }

    public void AddProductVariantOption(ProductVariantOption productVariantOption)
    {
        _productVariantOptions.Add(productVariantOption);
    }

    public void RemoveProductVariantOption(ProductVariantOption productVariantOption)
    {
        _productVariantOptions.Remove(productVariantOption);
    }

    private void EnsureActive()
    {
        if (IsArchived)
        {
            throw new ProductDomainException(
                "Archived product variants cannot be assigned a position.",
                nameof(Position)
            );
        }
    }

    private static void ValidateDetails(
        string name,
        string? sku,
        int costPrice,
        int sellingPrice,
        int position
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProductDomainException("Product variant name is required.", nameof(name));
        }

        if (name.Length > MaxNameLength)
        {
            throw new ProductDomainException(
                $"Product variant name cannot exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        ValidateNonNegative(costPrice, nameof(costPrice));
        ValidateNonNegative(sellingPrice, nameof(sellingPrice));
        ValidatePosition(position);
    }

    private static void ValidatePosition(int position)
    {
        ValidateNonNegative(position, nameof(position));
    }

    private static void ValidateNonNegative(int value, string field)
    {
        if (value < 0)
        {
            throw new ProductDomainException($"{field} cannot be negative.", field);
        }
    }
}
