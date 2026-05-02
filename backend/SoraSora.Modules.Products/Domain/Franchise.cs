namespace SoraSora.Modules.Products.Domain;

public sealed class Franchise
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
