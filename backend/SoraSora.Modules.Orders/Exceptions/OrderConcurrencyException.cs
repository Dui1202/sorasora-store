using SoraSora.Shared.Exceptions;

public sealed class OrderConcurrencyException(string orderId, Exception? innerException = null)
    : ConflictException(
        $"Product {orderId} was changed by another operation. Reload it and try again.",
        ErrorCode,
        innerException
    )
{
    public const string ErrorCode = OrderErrorCodes.ConcurrencyConflict;
}
