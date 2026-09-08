public class OrderItem
{
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public string OrderId { get; private set; } = null!;
    public string InventoryItemId { get; private set; } = null!;
}
