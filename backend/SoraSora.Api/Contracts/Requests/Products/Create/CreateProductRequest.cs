using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Contracts.Requests.Products.Create;

public sealed record CreateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }

    // public DateTime? PreorderReleaseAt { get; init; }
    public bool IsPublished { get; init; }
    public string? BaseSku { get; init; }
    public int? BaseCostPrice { get; init; }
    public int? BaseSellingPrice { get; init; }
    public int? BaseQuantity { get; init; }
    public IReadOnlyCollection<CreateProductVariantRequest> ProductVariants { get; init; } = [];
    public required IReadOnlyCollection<CreateProductImageRequest> ProductImages { get; init; }
    public IReadOnlyCollection<CreateVariationRequest> Variations { get; init; } = [];
}
