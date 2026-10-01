using NUlid;
using SoraSora.Modules.Inventory.Exceptions;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Inventory.Domain;

public sealed class InventoryItem : IAuditable
{
    private readonly List<Stock> _stocks = [];
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ProductVariantId { get; private set; } = null!;
    public bool IsArchived { get; private set; }
    public IReadOnlyCollection<Stock> Stocks => _stocks;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int OnHandedQuantity => _stocks.Sum(x => x.OnHandedQuantity);
    public int ReservedQuantity => _stocks.Sum(x => x.ReservedQuantity);
    public int AvailableQuantity => _stocks.Sum(x => x.AvailableQuantity);

    private InventoryItem() { }

    public static InventoryItem Create(string productVariantId)
    {
        return new InventoryItem { ProductVariantId = productVariantId };
    }

    public void Archive()
    {
        IsArchived = true;
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
