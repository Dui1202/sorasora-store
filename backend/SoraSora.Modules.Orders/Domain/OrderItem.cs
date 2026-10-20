using SoraSora.Modules.Orders.Exceptions;

namespace SoraSora.Modules.Orders.Domain;

public class OrderItem
{
    public const int MaxNameLength = 255;
    public const int MaxSkuLength = 40;
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public string ProductName { get; private set; } = null!;
    public string? Sku { get; private set; }
    public string OrderId { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    public string ProductVariantId { get; private set; } = null!;
    public int SellingPrice { get; private set; }
    public int Quantity { get; private set; }
    public OrderSaleType SaleType { get; private set; }
    public string WarehouseId { get; private set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(
        string productName,
        string productVariantId,
        int sellingPrice,
        int quantity,
        OrderSaleType saleType,
        string warehouseId,
        string? sku = null
    )
    {
        if (string.IsNullOrWhiteSpace(productVariantId))
        {
            throw new OrderDomainException(
                "Product variant ID is required.",
                nameof(productVariantId)
            );
        }

        if (string.IsNullOrWhiteSpace(warehouseId))
        {
            throw new OrderDomainException("Warehouse ID is required.", nameof(warehouseId));
        }

        ValidateDetails(productName, sellingPrice, quantity, saleType, sku);

        return new OrderItem
        {
            ProductName = productName,
            ProductVariantId = productVariantId,
            SellingPrice = sellingPrice,
            Quantity = quantity,
            SaleType = saleType,
            WarehouseId = warehouseId,
            Sku = sku,
        };
    }

    public void UpdateDetails(
        int sellingPrice,
        int quantity,
        OrderSaleType saleType,
        string? sku = null
    )
    {
        ValidateDetails(null, sellingPrice, quantity, saleType, sku);

        SellingPrice = sellingPrice;
        Quantity = quantity;
        SaleType = saleType;
    }

    private static void ValidateDetails(
        string? productName,
        int sellingPrice,
        int quantity,
        OrderSaleType saleType,
        string? sku = null
    )
    {
        if (productName?.Length > MaxNameLength)
        {
            throw new OrderDomainException(
                $"Product name cannot exceed {MaxNameLength} characters.",
                nameof(productName)
            );
        }

        if (sellingPrice < 0)
        {
            throw new OrderDomainException(
                "Selling price cannot be negative.",
                nameof(sellingPrice)
            );
        }

        if (quantity <= 0)
        {
            throw new OrderDomainException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (sku?.Length > MaxSkuLength)
        {
            throw new OrderDomainException(
                $"Sku length cannot exceed {MaxSkuLength} characters",
                nameof(sku)
            );
        }

        if (!Enum.IsDefined(saleType))
        {
            throw new OrderDomainException(
                $"{saleType} is not defined in OrderSaleType enum",
                nameof(saleType)
            );
        }
    }
}
