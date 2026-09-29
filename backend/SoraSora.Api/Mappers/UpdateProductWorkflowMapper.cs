using NUlid;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Api.Contracts.Requests.Products.Update;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;

namespace SoraSora.Api.Mappers;

public static class UpdateProductWorkflowMapper
{
    public static UpdateInventoryItemCommand ToUpdateInventoryItemCommand(
        RetainedProductVariantWorkflowCommand command
    )
    {
        return new UpdateInventoryItemCommand
        {
            ProductVariantId = command.Id,
            Action = command.Action,
            Quantity = command.Quantity,
        };
    }

    public static UpdateProductCommand ToUpdateProductCommand(UpdateProductWorkflowCommand command)
    {
        return new UpdateProductCommand
        {
            Id = command.Id,
            Name = command.Name,
            Version = command.Version,
            Description = command.Description,
            SupplierCountry = command.SupplierCountry,
            SellMode = command.SellMode,
            IsPublished = command.IsPublished,
            NewImages = command
                .NewImages.Select(CreateProductWorkflowMapper.ToCreateProductImageCommand)
                .ToList(),
            RetainedImages = command.RetainedImages,
            NewProductVariants = command
                .NewProductVariants.Select(ToCreateProductVariantCommand)
                .ToList(),
            RetainedProductVariants = command
                .RetainedProductVariants.Select(ToRetainedProductVariantCommand)
                .ToList(),
            Variations = command.Variations,
        };
    }

    public static UpdateProductWorkflowCommand ToUpdateProductWorkflowCommand(
        UpdateProductRequest request,
        IDictionary<string, IFormFile> productImageFilesByClientId,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new UpdateProductWorkflowCommand
        {
            Id = request.Id,
            Version = request.Version,
            Name = request.Name,
            Description = request.Description,
            IsPublished = request.IsPublished,
            SupplierCountry = request.SupplierCountry,
            SellMode = request.SellMode,
            NewImages = request
                .NewImages.Select(image =>
                    CreateProductWorkflowMapper.ToCreateProductImageWorkflowCommand(
                        image,
                        productImageFilesByClientId
                    )
                )
                .ToList(),
            RetainedImages = request.RetainedImages.Select(ToRetainedProductImageCommand).ToList(),
            NewProductVariants = request
                .NewProductVariants.Select(variant =>
                    CreateProductWorkflowMapper.ToCreateProductVariantWorkflowCommand(
                        variant,
                        variantImageFilesByClientId
                    )
                )
                .ToList(),
            RetainedProductVariants = request
                .RetainedProductVariants.Select(variant =>
                    ToRetainedProductVariantWorkflowCommand(variant, variantImageFilesByClientId)
                )
                .ToList(),
            Variations = request.Variations.Select(ToUpdateVariationCommand).ToList(),
        };
    }

    private static CreateProductVariantCommand ToCreateProductVariantCommand(
        CreateProductVariantWorkflowCommand command
    )
    {
        return new CreateProductVariantCommand
        {
            ClientId = command.ClientId,
            Name = command.Name,
            Sku = command.Sku,
            CostPrice = command.CostPrice,
            SellingPrice = command.SellingPrice,
            Position = command.Position,
            ProductVariantImage = CreateProductWorkflowMapper.ToCreateProductVariantImageCommand(
                command.ProductVariantImage
            ),
            ProductVariantOptions = command.ProductVariantOptions,
        };
    }

    private static RetainedProductVariantCommand ToRetainedProductVariantCommand(
        RetainedProductVariantWorkflowCommand command
    )
    {
        return new RetainedProductVariantCommand
        {
            Id = command.Id,
            Name = command.Name,
            Sku = command.Sku,
            CostPrice = command.CostPrice,
            SellingPrice = command.SellingPrice,
            Position = command.Position,
            NewImage = command.NewImage is null
                ? null
                : CreateProductWorkflowMapper.ToCreateProductVariantImageCommand(command.NewImage),
            RetainedImage = command.RetainedImage,
            ProductVariantOptions = command.ProductVariantOptions,
        };
    }

    private static RetainedProductImageCommand ToRetainedProductImageCommand(
        RetainedProductImageRequest request
    )
    {
        return new RetainedProductImageCommand(
            MediaAssetId: request.MediaAssetId,
            Alt: request.Alt,
            Position: request.Position
        );
    }

    private static RetainedProductVariantWorkflowCommand ToRetainedProductVariantWorkflowCommand(
        RetainedProductVariantRequest request,
        IDictionary<string, IFormFile> variantImageFilesByClientId
    )
    {
        return new RetainedProductVariantWorkflowCommand
        {
            Id = request.Id,
            Action = request.Action,
            Quantity = request.Quantity,
            Name = request.Name,
            Sku = request.Sku,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            Position = request.Position,
            NewImage = request.NewImage is null
                ? null
                : CreateProductWorkflowMapper.ToCreateProductVariantImageWorkflowCommand(
                    request.NewImage,
                    variantImageFilesByClientId
                ),
            RetainedImage = request.RetainedImage is null
                ? null
                : ToRetainedProductVariantImageCommand(request.RetainedImage),
            ProductVariantOptions = request
                .ProductVariantOptions.Select(ToCreateProductVariantOptionCommand)
                .ToList(),
        };
    }

    private static RetainedProductVariantImageCommand ToRetainedProductVariantImageCommand(
        RetainedProductVariantImageRequest request
    )
    {
        return new RetainedProductVariantImageCommand
        {
            MediaAssetId = request.MediaAssetId,
            Alt = request.Alt,
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

    private static UpdateVariationCommand ToUpdateVariationCommand(UpdateVariationRequest request)
    {
        return new UpdateVariationCommand
        {
            Id = request.Id,
            ClientId = request.ClientId,
            Name = request.Name,
            Position = request.Position,
            VariationOptions = request
                .VariationOptions.Select(ToUpdateVariationOptionCommand)
                .ToList(),
        };
    }

    private static UpdateVariationOptionCommand ToUpdateVariationOptionCommand(
        UpdateVariationOptionRequest request
    )
    {
        return new UpdateVariationOptionCommand
        {
            Id = request.Id,
            ClientId = request.ClientId,
            Value = request.Value,
            Position = request.Position,
        };
    }
}
