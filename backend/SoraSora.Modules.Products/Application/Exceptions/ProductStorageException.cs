using SoraSora.Modules.Products.Contracts;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Products.Application.Exceptions;

public sealed class ProductStorageException(Exception innerException)
    : ExternalServiceException(
        "Product image storage is temporarily unavailable.",
        ProductErrorCodes.StorageUnavailable,
        innerException
    );
