using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductVariationOwnershipException(string variationId, string productId)
    : BadRequestException(
        $"Variation {variationId} does not belong to product {productId}.",
        ProductErrorCodes.VariationNotOwned
    );
