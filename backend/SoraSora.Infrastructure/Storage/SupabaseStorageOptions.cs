namespace SoraSora.Infrastructure.Storage
{
    public sealed class SupabaseStorageOptions
    {
        public string Bucket { get; init; } = string.Empty;
        public string Region { get; init; } = string.Empty;
        public string Endpoint { get; init; } = string.Empty;
        public string AccessKeyId { get; init; } = string.Empty;
        public string SecretAccessKey { get; init; } = string.Empty;
        public string PublicBaseUrl { get; init; } = string.Empty;
    }
}
