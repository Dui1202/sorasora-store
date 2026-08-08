namespace SoraSora.Modules.Media.Domain;

public sealed record ProcessedImage(byte[] Content, ImageFormat Format, int Width, int Height)
{
    public string ContentType =>
        Format switch
        {
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Png => "image/png",
            ImageFormat.WebP => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
        };
}
