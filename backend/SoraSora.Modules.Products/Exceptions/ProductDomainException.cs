using Microsoft.AspNetCore.Http.HttpResults;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductDomainException(string message, string? field = null)
    : BadRequestException(
        message + (field is not null ? ": " + field : null),
        ProductErrorCodes.DomainException
    )
{
    public string? Field { get; } = field;
}
