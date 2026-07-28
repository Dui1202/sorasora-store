namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductVariantCommand(
    string Name,
    string? Sku,
    int CostPrice,
    int SellingPrice,
    int Position,
    CreateProductVariantImageCommand ProductVariantImage
);
