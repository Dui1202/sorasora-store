using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Requests.Update
{
    public sealed record UpdateProductRequest
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public SupplierCountry SupplierCountry { get; init; }
        public bool IsPublished { get; init; }
        public bool HasVariation { get; init; }
        public required string BaseSku { get; init; }
        public int BaseCostPrice { get; init; }
        public int BaseSellingPrice { get; init; }
    }
}
