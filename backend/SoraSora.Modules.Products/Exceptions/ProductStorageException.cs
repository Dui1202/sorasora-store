using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Exceptions;

public sealed class ProductStorageException(Exception innerException)
    : ExternalServiceException(
        "Product image storage is temporarily unavailable.",
        ProductErrorCodes.StorageUnavailable,
        innerException
    );
