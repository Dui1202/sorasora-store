namespace SoraSora.Modules.Products.Contracts.Responses
{
    public sealed record ProductDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public bool IsPublished { get; init; }
        public IReadOnlyList<ProductVariantDto> Variants { get; init; } = [];
        public IReadOnlyList<ProductImageDto> Images { get; init; } = [];
    }
}
