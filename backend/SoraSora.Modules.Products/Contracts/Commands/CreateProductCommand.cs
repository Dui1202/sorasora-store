using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Commands
{
    public sealed record CreateProductCommand(
        string Name,
        string? Description,
        SupplierCountry SupplierCountry,
        bool IsPublished,
        IReadOnlyCollection<CreateProductImageCommand> ProductImages
    );
}
