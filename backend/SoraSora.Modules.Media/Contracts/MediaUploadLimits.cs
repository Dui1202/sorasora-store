namespace SoraSora.Modules.Media.Contracts
{
    public static class MediaUploadLimits
    {
        public const long MaxImageBytes = 15 * 1024 * 1024;
        public const int MaxImageWidth = 8192;
        public const int MaxImageHeight = 8192;
        public const long MaxImagePixels = 40_000_000;
    }
}
