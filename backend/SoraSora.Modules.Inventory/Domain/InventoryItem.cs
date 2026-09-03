using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Inventory.Domain;

public sealed class InventoryItem : IAuditable
{
    private readonly List<Stock> _stocks = [];
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string? Sku { get; private set; }
    public string ProductVariantId { get; private set; } = null!;
    public IReadOnlyCollection<Stock> Stocks => _stocks;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int OnHandedQuantity => _stocks.Sum(x => x.OnHandedQuantity);
    public int ReservedQuantity => _stocks.Sum(x => x.ReservedQuantity);
    public int AvailableQuantity => _stocks.Sum(x => x.AvailableQuantity);

    private InventoryItem() { }

    public static InventoryItem Create(string productVariantId, string? sku = null)
    {
        return new InventoryItem { ProductVariantId = productVariantId, Sku = sku };
    }

    public void AddStock(Stock stock)
    {
        if (_stocks.Any(x => x.WarehouseId == stock.WarehouseId))
        {
            throw new InventoryDomainException(
                $"Inventory already has stock for this warehouse with id: {stock.WarehouseId} "
            );
        }
        _stocks.Add(stock);
    }
}
