using NUlid;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductVariantImage
{
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ProductVariantId { get; private set; } = null!;
    public string MediaAssetId { get; private set; } = null!;
    public ProductVariant ProductVariant { get; private set; } = null!;

    private ProductVariantImage() { }

    public static ProductVariantImage Create(string mediaAssetId)
    {
        return new ProductVariantImage { MediaAssetId = mediaAssetId };
    }
}
