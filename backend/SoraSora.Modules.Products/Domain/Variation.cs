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

    private Variation() { }

    public static Variation Create(string name, int position)
    {
        ValidateDetails(name, position);
        return new Variation { Name = name, Position = position };
    }

    public void UpdateDetails(string name, int position)
    {
        ValidateDetails(name, position);
        Name = name;
        Position = position;
    }

    public void AddOption(VariationOption option)
    {
        option.AttachTo(this);
        _variationOptions.Add(option);
    }

    public void RemoveOption(VariationOption option)
    {
        _variationOptions.Remove(option);
    }

    private static void ValidateDetails(string name, int position)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProductDomainException("Variation name is required.", nameof(name));
        }

        if (position < 0)
        {
            throw new ProductDomainException(
                "Variation position cannot be negative.",
                nameof(position)
            );
        }
    }
}
