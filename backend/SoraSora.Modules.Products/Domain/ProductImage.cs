using Amazon.S3.Model;
using NUlid;

namespace SoraSora.Modules.Products.Domain;

public sealed class ProductImage
{
    public const int MaxAltLength = 255;

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

        ValidateDetails(alt, position);

        return new ProductImage
        {
            Path = path,
            Alt = alt,
            Position = position,
        };
    }

    public void UpdateDetails(string? alt, int position)
    {
        ValidateDetails(alt, position);

        Alt = alt;
        Position = position;
    }

    public void UpdatePosition(int position)
    {
        ValidatePosition(position);
        Position = position;
    }

    private static void ValidateDetails(string? alt, int position)
    {
        if (alt?.Length > MaxAltLength)
        {
            throw new ArgumentException(
                $"Image alt text cannot exceed {MaxAltLength} characters.",
                nameof(alt)
            );
        }

        ArgumentOutOfRangeException.ThrowIfNegative(position);
    }

    private static void ValidatePosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
    }
}
