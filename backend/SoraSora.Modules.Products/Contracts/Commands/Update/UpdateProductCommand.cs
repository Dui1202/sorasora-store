using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record UpdateProductCommand
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsPublished { get; init; }
    public bool HasVariation { get; init; }
    public IReadOnlyCollection<CreateProductImageCommand> NewImages { get; init; } = [];
    public IReadOnlyCollection<RetainedProductImageCommand> RetainedImages { get; init; } = [];
    public IReadOnlyCollection<CreateProductVariantCommand> NewProductVariants { get; init; } = [];
    public IReadOnlyCollection<RetainedProductVariantCommand> RetainedProductVariants { get; init; } =
    [];
}
