using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Exceptions;

public sealed class OrderDomainException(string message, string? field = null)
    : BadRequestException(
        message + (field is not null ? ": " + field : null),
        "ORDER_DOMAIN_EXCEPTION"
    )
{
    public string? Field { get; } = field;
}
