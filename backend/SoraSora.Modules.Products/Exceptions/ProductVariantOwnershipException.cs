using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductVariantOwnershipException(string productVariantId, string productId)
    : BadRequestException(
        $"Product variant {productVariantId} does not belong to product {productId}.",
        ProductErrorCodes.VariantNotOwned
    );
