using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Domain;

public sealed record CreateProductWorkflowCommand
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsPublished { get; init; }
    public required IReadOnlyCollection<CreateProductImageCommand> ProductImages { get; init; }
    public required IReadOnlyCollection<CreateProductVariantWorkflowCommand> ProductVariants { get; init; }
    public IReadOnlyCollection<CreateVariationCommand> Variations { get; init; } = [];
}
