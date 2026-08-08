using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductCommand
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsPublished { get; init; }
    public bool HasVariation { get; init; }
    public required IReadOnlyCollection<CreateProductImageCommand> ProductImages { get; init; }
    public required IReadOnlyCollection<CreateProductVariantCommand> ProductVariants { get; init; }
    public IReadOnlyCollection<CreateVariationCommand> Variations { get; init; } = [];
};
