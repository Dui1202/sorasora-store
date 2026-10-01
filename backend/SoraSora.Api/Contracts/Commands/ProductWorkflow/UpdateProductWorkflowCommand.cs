using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Contracts.Commands.ProductWorkflow;

public sealed record UpdateProductWorkflowCommand
{
    public string Id { get; init; } = string.Empty;
    public Guid Version { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsPublished { get; init; }
    public IReadOnlyCollection<CreateProductImageWorkflowCommand> NewImages { get; init; } = [];
    public IReadOnlyCollection<RetainedProductImageCommand> RetainedImages { get; init; } = [];
    public IReadOnlyCollection<CreateProductVariantWorkflowCommand> NewProductVariants { get; init; } =
    [];
    public IReadOnlyCollection<RetainedProductVariantWorkflowCommand> RetainedProductVariants { get; init; } =
    [];
    public IReadOnlyCollection<UpdateVariationCommand> Variations { get; init; } = [];
}
