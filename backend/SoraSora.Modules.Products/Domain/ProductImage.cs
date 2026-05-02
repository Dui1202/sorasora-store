namespace SoraSora.Modules.Products.Domain;

public sealed class ProductImage
{
    public string Id { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public string? Alt { get; set; }

    public int Position { get; set; } = 0;

    public Boolean IsCover { get; set; } = false;

}
