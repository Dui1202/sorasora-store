using Amazon.S3.Model;
using NUlid;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductImage
{
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string ProductId { get; private set; } = null!;
    public string Path { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public string? Alt { get; private set; }
    public int Position { get; private set; } = 0;

    internal static ProductImage Create(string path, string? alt, int position)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Image path is required!", nameof(path));
        }

        return new ProductImage
        {
            Path = path,
            Alt = alt,
            Position = position,
        };
    }

    public void UpdateDetails(string? alt, int position)
    {
        Alt = alt;
        Position = position;
    }
}
