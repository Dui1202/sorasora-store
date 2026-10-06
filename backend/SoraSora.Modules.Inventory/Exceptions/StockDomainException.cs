using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Inventory.Exceptions;

public sealed class StockDomainException(string message, string? field = null)
    : BadRequestException(
        message + (field is not null ? ": " + field : null),
        InventoryErrorCodes.StockDomainException
    )
{
    public string? Field { get; } = field;
}
