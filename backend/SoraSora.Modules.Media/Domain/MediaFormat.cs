using System.Net.Mime;

namespace SoraSora.Modules.Media.Domain
{
    public readonly record struct MediaFormat
    {
        public MediaType Type { get; }

        public ImageFormat? Image { get; }

        public VideoFormat? Video { get; }

        private MediaFormat(ImageFormat format)
        {
            Type = MediaType.Image;
            Image = format;
            Video = null;
        }

        private MediaFormat(VideoFormat format)
        {
            Type = MediaType.Video;
            Image = null;
            Video = format;
        }

        public static MediaFormat ContentTypeToMediaFormat(string contentType)
        {
            return contentType switch
            {
                "image/webp" => new MediaFormat(ImageFormat.WebP),
                "image/jpeg" => new MediaFormat(ImageFormat.Jpeg),
                "image/png" => new MediaFormat(ImageFormat.Png),
                _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null),
            };
        }

        public static implicit operator MediaFormat(ImageFormat format)
        {
            return new MediaFormat(format);
        }

        public static implicit operator MediaFormat(VideoFormat format)
        {
            return new MediaFormat(format);
        }
    }
}
