namespace SoraSora.Modules.Products.Application.Images;

public sealed record ProcessedProductImage(
    byte[] Content,
    ProductImageFormat Format,
    int Width,
    int Height
)
{
    public string ContentType =>
        Format switch
        {
            ProductImageFormat.Jpeg => "image/jpeg",
            ProductImageFormat.Png => "image/png",
            ProductImageFormat.WebP => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
        };
}
