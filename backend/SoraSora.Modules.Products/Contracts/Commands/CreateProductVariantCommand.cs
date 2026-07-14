namespace SoraSora.Modules.Products.Contracts.Commands
{
    public sealed record CreateProductVariantCommand(
        string Name,
        string Sku,
        int CostPrice,
        int SellingPrice,
        int Position,
        bool IsArchived,
        CreateProductVariantImageCommand ProductVariantImage
    );
}
