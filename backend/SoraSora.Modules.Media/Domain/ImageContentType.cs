using Humanizer;

namespace SoraSora.Modules.Media.Domain
{
    public static class ImageContentType
    {
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        public const string WebP = "image/webp";

        public static string ToImageContentType(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Jpeg => Jpeg,
                ImageFormat.Png => Png,
                ImageFormat.WebP => WebP,
                _ => throw new InvalidOperationException($"Invalid image format: {format}"),
            };
        }
    }
}
