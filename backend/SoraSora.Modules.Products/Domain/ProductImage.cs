using Amazon.S3.Model;
using NUlid;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductImage
{
    public const int MaxAltLength = 255;

    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ProductId { get; private set; } = null!;
    public string MediaAssetId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public int Position { get; private set; } = 0;

    internal static ProductImage Create(string mediaAssetId, string? alt, int position)
    {
        if (string.IsNullOrWhiteSpace(mediaAssetId))
        {
            throw new ProductDomainException(
                "Image MediaAssetId is required.",
                nameof(mediaAssetId)
            );
        }

        ValidateDetails(position);

        return new ProductImage { MediaAssetId = mediaAssetId, Position = position };
    }

    public void UpdateDetails(int position)
    {
        ValidateDetails(position);

        Position = position;
    }

    private static void ValidateDetails(int position)
    {
        if (position < 0)
        {
            throw new ProductDomainException(
                "Image position cannot be negative.",
                nameof(position)
            );
        }
    }
}
