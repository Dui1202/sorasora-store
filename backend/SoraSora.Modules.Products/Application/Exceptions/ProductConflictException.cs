using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductConflictException(
    string message,
    Exception? innerException = null
) : ConflictException(message, ProductErrorCodes.Conflict, innerException);
