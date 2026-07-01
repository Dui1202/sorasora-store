using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Franchise : IAuditable
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<Product> Products { get; set; } = [];
    }
}
