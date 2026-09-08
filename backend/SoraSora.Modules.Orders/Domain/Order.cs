public sealed class Order
{
    private readonly List<OrderItem> _orderItem = [];
    public string Id { get; private set; } = NUlid.Ulid.NewUlid().ToString();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItem;
    
}
