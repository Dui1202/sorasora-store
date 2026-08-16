namespace SoraSora.Modules.Media.Contracts.Upload;

public abstract record MediaStorageTarget
{
    private MediaStorageTarget() { }

    public sealed record ProductImage : MediaStorageTarget
    {
        public string ProductId { get; }

        public ProductImage(string productId)
        {
            ValidatePathSegment(productId, nameof(productId));
            ProductId = productId;
        }
    }

    public sealed record ProductVariantImage : MediaStorageTarget
    {
        public string ProductId { get; }
        public string VariantId { get; }

        public ProductVariantImage(string productId, string variantId)
        {
            ValidatePathSegment(productId, nameof(productId));
            ValidatePathSegment(variantId, nameof(variantId));

            ProductId = productId;
            VariantId = variantId;
        }
    }

    private static void ValidatePathSegment(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        if (value.Contains('/') || value.Contains('\\'))
        {
            throw new ArgumentException(
                "Storage path identifiers must not contain path separators.",
                parameterName
            );
        }
    }
}
