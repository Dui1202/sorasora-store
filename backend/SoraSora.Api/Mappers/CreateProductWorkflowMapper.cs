using NUlid;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using SoraSora.Api.Contracts.Commands.ProductWorkflow;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Mappers;

internal static class CreateProductWorkflowMapper
{
    public static CreateProductWorkflowCommand ToCreateProductWorkflowCommand(
        CreateProductRequest request,
        IDictionary<string, IFormFile> productImageFilesByClientId,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new CreateProductWorkflowCommand
        {
            Name = request.Name,
            Description = request.Description,
            SupplierCountry = request.SupplierCountry,
            SellMode = request.SellMode,
            IsPublished = request.IsPublished,
            ProductImages = request
                .ProductImages.Select(image =>
                    ToCreateProductImageWorkflowCommand(image, productImageFilesByClientId)
                )
                .ToList(),
            Variations = request.Variations.Select(ToCreateVariationCommand).ToList(),
            ProductVariants = request
                .ProductVariants.Select(variant =>
                    ToCreateProductVariantWorkflowCommand(variant, variantImageFilesByClientId)
                )
                .ToList(),
        };
    }

    public static CreateProductImageWorkflowCommand ToCreateProductImageWorkflowCommand(
        CreateProductImageRequest request,
        IDictionary<string, IFormFile> productImageFilesByClientId
    )
    {
        return new CreateProductImageWorkflowCommand
        {
            ClientId = request.ClientId,
            File = GetImageFile(request.ClientId, productImageFilesByClientId),
            Alt = request.Alt,
            Position = request.Position,
        };
    }

    public static CreateProductVariantWorkflowCommand ToCreateProductVariantWorkflowCommand(
        CreateProductVariantRequest request,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new CreateProductVariantWorkflowCommand
        {
            ClientId = Ulid.NewUlid().ToString(),
            Name = request.Name,
            Sku = request.Sku,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Position = request.Position,
            InitialQuantity = request.Quantity,
            ProductVariantImage = ToCreateProductVariantImageWorkflowCommand(
                request.ProductVariantImage,
                variantImageFilesByClientId
            ),
            ProductVariantOptions = request
                .ProductVariantOptions.Select(ToCreateProductVariantOptionCommand)
                .ToList(),
        };
    }

    public static CreateProductVariantImageWorkflowCommand ToCreateProductVariantImageWorkflowCommand(
        CreateProductVariantImageRequest request,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new CreateProductVariantImageWorkflowCommand
        {
            ClientId = request.ClientId,
            File = GetImageFile(request.ClientId, variantImageFilesByClientId),
            Alt = request.Alt,
        };
    }

    public static IFormFile GetImageFile(
        string clientId,
        IDictionary<string, IFormFile> imageFilesByClientId
    )
    {
        if (!imageFilesByClientId.TryGetValue(clientId, out var file))
        {
            throw new InvalidOperationException(
                "Cannot get the image file with its corresponding client ID."
            );
        }

        return file;
    }

    public static CreateVariationCommand ToCreateVariationCommand(CreateVariationRequest request)
    {
        return new CreateVariationCommand
        {
            ClientId = request.ClientId,
            Name = request.Name,
            Position = request.Position,
            VariationOptions = request
                .VariationOptions.Select(ToCreateVariationOptionCommand)
                .ToList(),
        };
    }

    private static CreateVariationOptionCommand ToCreateVariationOptionCommand(
        CreateVariationOptionRequest request
    )
    {
        return new CreateVariationOptionCommand
        {
            ClientId = request.ClientId,
            Value = request.Value,
            Position = request.Position,
        };
    }

    private static CreateProductVariantOptionCommand ToCreateProductVariantOptionCommand(
        CreateProductVariantOptionRequest request
    )
    {
        return new CreateProductVariantOptionCommand
        {
            VariationClientId = request.VariationClientId,
            VariationOptionClientId = request.VariationOptionClientId,
        };
    }

    internal static CreateProductCommand ToCreateProductCommand(
        CreateProductWorkflowCommand command
    )
    {
        return new CreateProductCommand
        {
            Name = command.Name,
            Description = command.Description,
            SupplierCountry = command.SupplierCountry,
            SellMode = command.SellMode,
            IsPublished = command.IsPublished,
            ProductImages = command.ProductImages.Select(ToCreateProductImageCommand).ToList(),
            Variations = command.Variations,
            ProductVariants = command
                .ProductVariants.Select(variant => new CreateProductVariantCommand
                {
                    ClientId = variant.ClientId,
                    Name = variant.Name,
                    Sku = variant.Sku,
                    CostPrice = variant.CostPrice,
                    SellingPrice = variant.SellingPrice,
                    Position = variant.Position,
                    ProductVariantImage = ToCreateProductVariantImageCommand(
                        variant.ProductVariantImage
                    ),
                    ProductVariantOptions = variant.ProductVariantOptions,
                })
                .ToList(),
        };
    }

    internal static CreateProductImageCommand ToCreateProductImageCommand(
        CreateProductImageWorkflowCommand command
    )
    {
        return new CreateProductImageCommand(
            ClientId: command.ClientId,
            Alt: command.Alt,
            Position: command.Position
        );
    }

    internal static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
        CreateProductVariantImageWorkflowCommand command
    )
    {
        return new CreateProductVariantImageCommand(Alt: command.Alt);
    }

    internal static CreateInventoryItemCommand ToCreateInventoryItemCommand(
        ProductVariant variant,
        CreateProductVariantWorkflowCommand command
    )
    {
        return new CreateInventoryItemCommand(
            ProductVariantId: variant.Id,
            InitialQuantity: command.InitialQuantity
        );
    }
}
