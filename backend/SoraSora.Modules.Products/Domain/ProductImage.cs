namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductImage
    {
        public required string Id { get; set; }

        public required string ProductId { get; set; }

        public required string Path { get; set; }

        public required Product Product { get; set; }

        public string? Alt { get; set; }

        public required int Position { get; set; } = 0;
    }
}
