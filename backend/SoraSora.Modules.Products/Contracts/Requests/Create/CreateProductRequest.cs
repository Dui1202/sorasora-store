using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Requests.Create
{
    public sealed record CreateProductRequest
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required SupplierCountry SupplierCountry { get; init; }

        // public SellMode SellMode { get; init; }
        // public DateTime? PreorderReleaseAt { get; init; }
        public bool IsPublished { get; init; }
        public required IReadOnlyCollection<CreateProductImageRequest> ProductImages { get; init; }
        public bool HasVariation { get; init; }
        public required string BaseSku { get; init; }
        public int BaseCostPrice { get; init; }
        public int BaseSellingPrice { get; init; }
        public required IReadOnlyCollection<CreateProductVariantRequest> ProductVariants { get; init; }
    }
}
