using SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record RetainedProductVariantWorkflowCommand
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public int CostPrice { get; init; }
    public int SellingPrice { get; init; }
    public int Position { get; init; }
    public InventoryUpdateAction Action { get; init; }
    public int Quantity { get; init; }
    public CreateProductVariantImageWorkflowCommand? NewImage { get; init; }
    public RetainedProductVariantImageCommand? RetainedImage { get; init; }
    public IReadOnlyCollection<CreateProductVariantOptionCommand> ProductVariantOptions { get; init; } =
    [];
}
