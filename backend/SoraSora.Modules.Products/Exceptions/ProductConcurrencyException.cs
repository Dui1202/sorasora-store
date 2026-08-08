using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductConcurrencyException(string productId, Exception? innerException = null)
    : ConflictException(
        $"Product {productId} was changed by another operation. Reload it and try again.",
        ErrorCode,
        innerException
    )
{
    public const string ErrorCode = ProductErrorCodes.ConcurrencyConflict;
}
