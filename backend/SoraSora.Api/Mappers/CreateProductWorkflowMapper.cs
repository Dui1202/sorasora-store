using NUlid;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Api.Mappers;

internal static class CreateProductWorkflowMapper
{
    internal static CreateProductWorkflowCommand ToCreateProductWorkflowCommand(
        CreateProductRequest request
    )
    {
        var productCommand = CreateProductDtoMapper.ToCreateProductCommand(request);
        var quantitiesByPosition = request.ProductVariants.ToDictionary(
            variant => variant.Position,
            variant => variant.Quantity
        );

        return new CreateProductWorkflowCommand
        {
            Name = productCommand.Name,
            Description = productCommand.Description,
            SupplierCountry = productCommand.SupplierCountry,
            SellMode = productCommand.SellMode,
            IsPublished = productCommand.IsPublished,
            ProductImages = productCommand.ProductImages,
            Variations = productCommand.Variations,
            ProductVariants = productCommand.ProductVariants
                .Select(variant => ToCreateProductVariantWorkflowCommand(
                    variant,
                    request.Variations.Count == 0
                        ? request.BaseQuantity ?? 0
                        : quantitiesByPosition[variant.Position]
                ))
                .ToList(),
        };
    }

    private static CreateProductVariantWorkflowCommand ToCreateProductVariantWorkflowCommand(
        CreateProductVariantCommand command,
        int initialQuantity
    )
    {
        return new CreateProductVariantWorkflowCommand
        {
            ClientId = Ulid.NewUlid().ToString(),
            Name = command.Name,
            Sku = command.Sku,
            CostPrice = command.CostPrice,
            SellingPrice = command.SellingPrice,
            Position = command.Position,
            InitialQuantity = initialQuantity,
            ProductVariantImage = command.ProductVariantImage,
            ProductVariantOptions = command.ProductVariantOptions,
        };
    }

    internal static CreateProductCommand ToCreateProductCommand(CreateProductWorkflowCommand command)
    {
        return new CreateProductCommand
        {
            Name = command.Name,
            Description = command.Description,
            SupplierCountry = command.SupplierCountry,
            SellMode = command.SellMode,
            IsPublished = command.IsPublished,
            ProductImages = command.ProductImages,
            Variations = command.Variations,
            ProductVariants = command.ProductVariants
                .Select(variant => new CreateProductVariantCommand
                {
                    ClientId = variant.ClientId,
                    Name = variant.Name,
                    Sku = variant.Sku,
                    CostPrice = variant.CostPrice,
                    SellingPrice = variant.SellingPrice,
                    Position = variant.Position,
                    ProductVariantImage = variant.ProductVariantImage,
                    ProductVariantOptions = variant.ProductVariantOptions,
                })
                .ToList(),
        };
    }

    internal static CreateInventoryItemCommand ToCreateInventoryItemCommand(
        ProductVariantResponse variant,
        CreateProductVariantWorkflowCommand command
    )
    {
        return new CreateInventoryItemCommand(
            ProductVariantId: variant.Id,
            Sku: variant.Sku,
            InitialQuantity: command.InitialQuantity
        );
    }
}
