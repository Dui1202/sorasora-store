using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain;

public sealed class VariationOption : IAuditable
{
    private readonly List<ProductVariantOption> _productVariantOptions = [];
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string VariationId { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int Position { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Variation Variation { get; set; } = null!;
    public IReadOnlyCollection<ProductVariantOption> ProductVariantOptions =>
        _productVariantOptions;

    private VariationOption() { }

    public static VariationOption Create(string value, int position)
    {
        return new VariationOption { Value = value, Position = position };
    }
}
