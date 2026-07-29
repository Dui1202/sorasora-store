using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductImageOwnershipException(string imagePath, string productId)
    : BadRequestException(
        $"Image with path {imagePath} does not belong to product {productId}.",
        ProductErrorCodes.ImageNotOwned
    );
