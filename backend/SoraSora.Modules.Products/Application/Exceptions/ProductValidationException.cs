using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductValidationException(
    string message,
    Exception? innerException = null
) : BadRequestException(message, ProductErrorCodes.ValidationFailed, innerException);
