using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class Variation : IAuditable
    {
        public string Id { get; private set; } = null!;

        public string ProductId { get; private set; } = null!;

        public Product Product { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public int Position { get; private set; }
        public ICollection<VariationOption> VariationOptions { get; private set; } = [];
    }
}
