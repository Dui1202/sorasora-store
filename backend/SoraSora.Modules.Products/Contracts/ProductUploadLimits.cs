namespace SoraSora.Modules.Products.Contracts;

public static class ProductUploadLimits
{
    public const int MaxProductImages = 10;
    public const int MaxProductVariants = 20;
    public const long MaxImageBytes = 5 * 1024 * 1024;
    public const long MaxRequestBytes = 160 * 1024 * 1024;
}
