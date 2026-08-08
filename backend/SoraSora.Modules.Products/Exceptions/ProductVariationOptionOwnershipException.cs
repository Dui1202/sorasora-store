using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions
{
    public sealed class ProductVariationOptionOwnershipException(
        string variationOptionId,
        string variationId
    )
        : BadRequestException(
            $"Variation {variationOptionId} does not belong to variation {variationId}.",
            ProductErrorCodes.VariationOptionNotOwned
        );
}
