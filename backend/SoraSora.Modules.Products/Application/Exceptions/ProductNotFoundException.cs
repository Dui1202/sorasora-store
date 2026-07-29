using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductNotFoundException(string productId)
    : NotFoundException(
        $"Product {productId} was not found.",
        ProductErrorCodes.NotFound
    );
