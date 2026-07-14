using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Commands
{
    public sealed record CreateProductCommand(
        string Name,
        string? Description,
        SupplierCountry SupplierCountry,
        SellMode SellMode,
        bool IsPublished,
        bool IsArchived,
        bool HasVariation,
        IReadOnlyCollection<CreateProductImageCommand> ProductImages,
        IReadOnlyCollection<CreateProductVariantCommand> ProductVariants
    );
}
