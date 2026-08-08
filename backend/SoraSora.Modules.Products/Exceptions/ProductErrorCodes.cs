namespace SoraSora.Modules.Products.Exceptions;

public static class ProductErrorCodes
{
    public const string NotFound = "PRODUCT_NOT_FOUND";
    public const string ValidationFailed = "PRODUCT_VALIDATION_FAILED";
    public const string VariantNotOwned = "PRODUCT_VARIANT_NOT_OWNED";
    public const string VariationNotOwned = "PRODUCT_VARIATION_NOT_OWNED";
    public const string VariationOptionNotOwned = "PRODUCT_VARIATION_OPTION_NOT_OWNED";
    public const string ImageNotOwned = "PRODUCT_IMAGE_NOT_OWNED";
    public const string Conflict = "PRODUCT_CONFLICT";
    public const string ConcurrencyConflict = "PRODUCT_CONCURRENCY_CONFLICT";
    public const string StorageUnavailable = "PRODUCT_STORAGE_UNAVAILABLE";
}
