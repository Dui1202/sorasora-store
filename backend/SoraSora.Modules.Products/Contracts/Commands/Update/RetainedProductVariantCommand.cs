using SoraSora.Modules.Products.Contracts.Commands.Create;

namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record RetainedProductVariantCommand
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public string? Sku { get; init; }

    public int CostPrice { get; init; }

    public int SellingPrice { get; init; }

    public int Position { get; init; }

    public CreateProductVariantImageCommand? NewImage { get; init; }
}
