using SoraSora.Shared.Exceptions;
using SoraSora.Modules.Products.Contracts;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductConcurrencyException(string productId, Exception? innerException = null)
    : ConflictException(
        $"Product {productId} was changed by another operation. Reload it and try again.",
        ErrorCode,
        innerException
    )
{
    public const string ErrorCode = ProductErrorCodes.ConcurrencyConflict;
}
