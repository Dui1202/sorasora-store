using NUlid;

namespace SoraSora.Modules.Inventory.Domain
{
    public class Stock
    {
        private readonly List<StockMovement> _stockMovements = [];
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public int OnHandedQuantity { get; private set; } = 0;
        public int ReservedQuantity { get; private set; } = 0;
        public InventoryItem InventoryItem { get; private set; } = null!;
        public string InventoryItemId { get; private set; } = null!;
        public string WarehouseId { get; private set; } = null!;
        public IReadOnlyCollection<StockMovement> StockMovements => _stockMovements;
        public int AvailableQuantity => OnHandedQuantity - ReservedQuantity;

        private Stock() { }

        public static Stock Create(string warehouseId, int receiveQuantity = 0)
        {
            var stock = new Stock { WarehouseId = warehouseId };
            stock.Receive(receiveQuantity, "Stock initialization");

            return stock;
        }

        public void Receive(int quantity, string? description = null)
        {
            ValidateQuantity(quantity);
            OnHandedQuantity += quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Receive,
                    onHandedDelta: quantity,
                    reservedDelta: 0,
                    description: description
                )
            );
        }

        public void Reserve(int quantity, string? description = null)
        {
            ValidateQuantity(quantity);

            ReservedQuantity += quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Reserve,
                    onHandedDelta: 0,
                    reservedDelta: quantity,
                    description: description
                )
            );
        }

        public void Release(int quantity, string? description = null)
        {
            ValidateQuantity(quantity);

            ReservedQuantity -= quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Release,
                    onHandedDelta: 0,
                    reservedDelta: -quantity,
                    description: description
                )
            );
        }

        public void Sale(int quantity, string? description = null)
        {
            ValidateQuantity(quantity);

            OnHandedQuantity -= quantity;
            ReservedQuantity -= quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Sale,
                    onHandedDelta: -quantity,
                    reservedDelta: -quantity,
                    description: description
                )
            );
        }

        public void Return(int quantity, string description)
        {
            ValidateQuantity(quantity);

            OnHandedQuantity += quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Return,
                    onHandedDelta: 0,
                    reservedDelta: -quantity,
                    description: description
                )
            );
        }

        public void AdjustOnHanded(int quantity, string description)
        {
            OnHandedQuantity += quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Adjustment,
                    onHandedDelta: quantity,
                    reservedDelta: 0,
                    description: description
                )
            );
        }

        public void AdjustReserved(int quantity, string description)
        {
            ReservedQuantity += quantity;
            _stockMovements.Add(
                StockMovement.Create(
                    stockMovementType: StockMovementType.Adjustment,
                    onHandedDelta: 0,
                    reservedDelta: quantity,
                    description: description
                )
            );
        }

        public void ValidateQuantity(int quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Stock quantity cannot be negative",
                    nameof(quantity)
                );
            }
        }
    }
}
