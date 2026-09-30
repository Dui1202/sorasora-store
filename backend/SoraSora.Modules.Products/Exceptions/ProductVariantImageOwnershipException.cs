using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductVariantImageOwnershipException(string mediaAssetId, string productVariantId)
    : BadRequestException(
        $"Media asset {mediaAssetId} does not belong to product variant {productVariantId}.",
        ProductErrorCodes.ImageNotOwned
    );
