namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record RetainedProductVariantImageCommand
{
    public required string MediaAssetId { get; init; }
    public string? Alt { get; init; }
}
