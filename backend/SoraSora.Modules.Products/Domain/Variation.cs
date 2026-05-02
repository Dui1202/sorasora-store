namespace SoraSora.Modules.Products.Domain;

public sealed class Variation
{
    public string Id { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
