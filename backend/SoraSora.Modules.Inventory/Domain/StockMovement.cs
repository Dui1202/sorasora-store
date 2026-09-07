using System.ComponentModel.DataAnnotations;
using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Inventory.Domain
{
    public class StockMovement : IAuditable
    {
        public const int MaxDescriptionLength = 2000;
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
            ValidateDetails(stockMovementType, description);

            return new StockMovement()
            {
                StockMovementType = stockMovementType,
                OnHandedDelta = onHandedDelta,
                ReservedDelta = reservedDelta,
                Description = description,
            };
        }

        private static void ValidateDetails(
            StockMovementType stockMovementType,
            string? description
        )
        {
            if (!Enum.IsDefined(stockMovementType))
            {
                throw new ArgumentOutOfRangeException(
                    "Stock movement type is invalid.",
                    nameof(stockMovementType)
                );
            }
            if (description?.Length > MaxDescriptionLength)
            {
                throw new ArgumentException(
                    $"Stock movement description cannot exceed {MaxDescriptionLength} characters.",
                    nameof(description)
                );
            }
        }
    }
}
