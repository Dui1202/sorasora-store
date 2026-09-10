using SoraSora.Modules.Orders.Exceptions;

public class OrderItem
{
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public string OrderId { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    public string ProductVariantId { get; private set; } = null!;
    public int SellingPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(string productVariantId, int sellingPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productVariantId))
        {
            throw new OrderDomainException(
                "Product variant ID is required.",
                nameof(productVariantId)
            );
        }
        ValidateDetails(sellingPrice, quantity);

        return new OrderItem
        {
            ProductVariantId = productVariantId,
            SellingPrice = sellingPrice,
            Quantity = quantity,
        };
    }

    public void UpdateDetails(int sellingPrice, int quantity)
    {
        ValidateDetails(sellingPrice, quantity);

        SellingPrice = sellingPrice;
        Quantity = quantity;
    }

    private static void ValidateDetails(int sellingPrice, int quantity)
    {
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
    }
}
