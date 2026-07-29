using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductVariant : IAuditable
{
    public const int MaxNameLength = 100;

    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ProductId { get; private set; } = null!;

    public string? Sku { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string Name { get; private set; } = null!;
    public int CostPrice { get; private set; }
    public int SellingPrice { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Position { get; private set; } = 0;
    public bool IsArchived { get; private set; }
    public ProductVariantImage? ProductVariantImage { get; private set; }

    // public required bool IsFullPaymentAllowed { get; set; } = true;
    // public required bool IsDepositRequired { get; set; } = false;
    // public int? DepositPrice { get; set; } = 0;
    private ProductVariant() { }

    public static ProductVariant Create(
        string name,
        string? sku,
        int costPrice,
        int sellingPrice,
        int position
    )
    {
        ValidateDetails(name, sku, costPrice, sellingPrice, position);

        return new ProductVariant
        {
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
        ValidateDetails(name, sku, costPrice, sellingPrice, position);

        Name = name;
        Sku = sku;
        CostPrice = costPrice;
        SellingPrice = sellingPrice;
        Position = position;
    }

    public void UpdatePosition(int position)
    {
        ValidatePosition(position);
        Position = position;
    }

    public void Archive()
    {
        IsArchived = true;
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
            throw new ArgumentException("Product variant name is required.", nameof(name));
        }

        if (name.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Product variant name cannot exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        ArgumentOutOfRangeException.ThrowIfNegative(costPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(sellingPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(position);
    }

    private static void ValidatePosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
    }
}
