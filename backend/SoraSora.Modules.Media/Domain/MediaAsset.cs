using NUlid;

namespace SoraSora.Modules.Media.Domain
{
    public sealed class MediaAsset
    {
        public const int MaxAltLength = 255;

        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string Path { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public string? Alt { get; private set; }

        private MediaAsset() { }

        public static MediaAsset Create(string path, string contentType, string? alt)
        {
            ValidateAlt(alt);

            return new MediaAsset
            {
                Path = path,
                ContentType = contentType,
                Alt = alt,
            };
        }

        public void UpdateAlt(string? alt)
        {
            ValidateAlt(alt);
            Alt = alt;
        }

        private static void ValidateAlt(string? alt)
        {
            if (alt?.Length > MaxAltLength)
            {
                throw new ArgumentException(
                    $"Media alt text cannot exceed {MaxAltLength} characters.",
                    nameof(alt)
                );
            }
        }
    }
}
