namespace SoraSora.Modules.Products.Contracts;

public static class ProductUploadLimits
{
    public const int MaxProductImages = 10;
    public const int MaxProductVariants = 20;
    public const long MaxImageBytes = 15 * 1024 * 1024;
    public const long MaxRequestBytes = 160 * 1024 * 1024;
    public const int MaxImageWidth = 8192;
    public const int MaxImageHeight = 8192;
    public const long MaxImagePixels = 40_000_000;
}
