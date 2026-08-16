using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductImageOwnershipException(string mediaAssetId, string productId)
    : BadRequestException(
        $"Media asset {mediaAssetId} does not belong to product {productId}.",
        ProductErrorCodes.ImageNotOwned
    );
