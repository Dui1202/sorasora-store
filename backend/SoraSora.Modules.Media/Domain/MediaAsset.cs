using NUlid;

namespace SoraSora.Modules.Media.Domain
{
    public sealed class MediaAsset
    {
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string Path { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;

        private MediaAsset() { }

        public static MediaAsset Create(string path, string contentType)
        {
            return new MediaAsset { Path = path, ContentType = contentType };
        }
    }
}
