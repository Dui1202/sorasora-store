using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Inventory.Domain
{
    public class StockMovement : IAuditable
    {
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public StockMovementType StockMovementType { get; private set; }
        public int OnHandedDelta { get; private set; }
        public int ReservedDelta { get; private set; }
        public string? Description { get; private set; }
        public string StockId { get; private set; } = null!;
        public Stock Stock { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private StockMovement() { }

        public static StockMovement Create(
            StockMovementType stockMovementType,
            int onHandedDelta,
            int reservedDelta,
            string? description = null
        )
        {
            return new StockMovement()
            {
                StockMovementType = stockMovementType,
                OnHandedDelta = onHandedDelta,
                ReservedDelta = reservedDelta,
                Description = description,
            };
        }
    }
}
