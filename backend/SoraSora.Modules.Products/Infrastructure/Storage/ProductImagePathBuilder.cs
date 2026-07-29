using SoraSora.Modules.Products.Application.Images;

namespace SoraSora.Modules.Products.Infrastructure.Storage;

public static class ProductImagePathBuilder
{
    public static string BuildProductImagePath(string productId, ProductImageFormat format)
    {
        return $"products/{productId}/images/{BuildImageFileName(format)}";
    }

    public static string BuildProductVariantImagePath(
        string productId,
        string productVariantId,
        ProductImageFormat format
    )
    {
        return $"products/{productId}/{productVariantId}/images/{BuildImageFileName(format)}";
    }

    private static string BuildImageFileName(ProductImageFormat format)
    {
        var extension = format switch
        {
            ProductImageFormat.Jpeg => ".jpg",
            ProductImageFormat.Png => ".png",
            ProductImageFormat.WebP => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        return $"{Guid.NewGuid()}{extension}";
    }
}
