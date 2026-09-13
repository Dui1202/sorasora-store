using NUlid;
using SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductVariantWorkflowCommand
{
    public required string ClientId { get; init; } = Ulid.NewUlid().ToString();
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public int CostPrice { get; init; }
    public int SellingPrice { get; init; }
    public int Position { get; init; }
    public int InitialQuantity { get; init; }
    public required CreateProductVariantImageCommand ProductVariantImage { get; init; }
    public IReadOnlyCollection<CreateProductVariantOptionCommand> ProductVariantOptions { get; init; } =
    [];
}
