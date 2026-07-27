using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class Franchise : IAuditable
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public ICollection<Product> Products { get; private set; } = [];
}
