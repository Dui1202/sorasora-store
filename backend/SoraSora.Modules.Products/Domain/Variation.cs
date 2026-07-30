using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class Variation : IAuditable
{
    private readonly List<VariationOption> _variationOptions = [];
    private readonly List<ProductVariantOption> _productVariantOptions = [];
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string ProductId { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Position { get; private set; }
    public IReadOnlyCollection<VariationOption> VariationOptions => _variationOptions;
}
