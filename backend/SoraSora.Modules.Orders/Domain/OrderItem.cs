using SoraSora.Modules.Orders.Exceptions;

public class OrderItem
{
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public string OrderId { get; private set; } = null!;
    public Order Order { get; private set; } = null!;
    public string InventoryItemId { get; private set; } = null!;
    public int SellingPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(string inventoryItemId, int sellingPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(inventoryItemId))
        {
            throw new OrderDomainException(
                "Inventory item ID is required.",
                nameof(inventoryItemId)
            );
        }
        ValidateDetails(sellingPrice, quantity);

        return new OrderItem
        {
            InventoryItemId = inventoryItemId,
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
