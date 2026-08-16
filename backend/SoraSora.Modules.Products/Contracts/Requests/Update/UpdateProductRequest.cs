using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Requests.Update;

public sealed record UpdateProductRequest
{
    public string Id { get; init; } = string.Empty;
    public Guid Version { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsPublished { get; init; }
    public IReadOnlyCollection<CreateProductImageRequest> NewImages { get; init; } = [];
    public IReadOnlyCollection<RetainedProductImageRequest> RetainedImages { get; init; } = [];
    public IReadOnlyCollection<CreateProductVariantRequest> NewProductVariants { get; init; } = [];
    public IReadOnlyCollection<RetainedProductVariantRequest> RetainedProductVariants { get; init; } =
    [];
    public IReadOnlyCollection<UpdateVariationRequest> Variations { get; init; } = [];
}
