namespace SoraSora.Modules.Products.Infrastructure.Storage;

public static class ProductImagePathBuilder
{
    public static string BuildProductImagePath(string productId, string contentType)
    {
        return $"products/{productId}/images/{BuildImageFileName(contentType)}";
    }

    public static string BuildProductVariantImagePath(
        string productId,
        string productVariantId,
        string contentType
    )
    {
        return $"products/{productId}/{productVariantId}/images/{BuildImageFileName(contentType)}";
    }

    private static string BuildImageFileName(string contentType)
    {
        var extension = contentType switch
        {
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null),
        };

        return $"{Guid.NewGuid()}{extension}";
    }
}
