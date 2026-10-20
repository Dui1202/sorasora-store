using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Responses
{
    public sealed record OrderableProductVariantDto
    {
        public SellMode SellMode { get; init; }
        public required string ProductName { get; init; }
        public required string ProductVariantName { get; init; }
        public required string ProductVariantId { get; init; }
        public int SellingPrice { get; init; }
        public string? Sku { get; init; }
    }
}
