using SoraSora.Modules.Products.Contracts.Commands;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Mappers
{
    internal static class CreateProductDtoMapper
    {
        private const int DefaultPosition = 0;
        private const bool DefaultIsArchived = false;

        internal static CreateProductCommand ToCreateProductCommand(CreateProductRequest request)
        {
            var productImages = request.ProductImages.Select(ToCreateProductImageCommand).ToList();

            var productVariants = request.HasVariation
                ? request.ProductVariants.Select(ToCreateProductVariantCommand).ToList()
                : [ToCreateProductVariantCommand(request)];

            return new CreateProductCommand(
                Name: request.Name,
                Description: request.Description,
                SupplierCountry: request.SupplierCountry,
                SellMode: request.SellMode,
                IsPublished: request.IsPublished,
                IsArchived: request.IsArchived,
                HasVariation: request.HasVariation,
                ProductImages: productImages,
                ProductVariants: productVariants
            );
        }

        private static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
            CreateProductImageRequest request
        )
        {
            return new CreateProductVariantImageCommand(File: request.File, Alt: request.Alt);
        }

        private static CreateProductImageCommand ToCreateProductImageCommand(
            CreateProductImageRequest request
        )
        {
            return new CreateProductImageCommand(
                File: request.File,
                Alt: request.Alt,
                Position: request.Position
            );
        }

        private static CreateProductVariantCommand ToCreateProductVariantCommand(
            CreateProductRequest request
        )
        {
            var baseImage =
                request.ProductImages.MinBy(image => image.Position)
                ?? throw new InvalidOperationException(
                    "A product without variations must contain at least one product image."
                );

            var costPrice =
                request.BaseCostPrice
                ?? throw new InvalidOperationException(
                    "A product without variations must have a base cost price."
                );

            var sellingPrice =
                request.BaseSellingPrice
                ?? throw new InvalidOperationException(
                    "A product without variations must have a base selling price."
                );

            var sku =
                request.BaseSku
                ?? throw new InvalidOperationException(
                    "A product without variations must have a base SKU."
                );

            return new CreateProductVariantCommand(
                Name: request.Name,
                Sku: sku,
                CostPrice: costPrice,
                SellingPrice: sellingPrice,
                Position: DefaultPosition,
                IsArchived: DefaultIsArchived,
                ProductVariantImage: ToCreateProductVariantImageCommand(baseImage)
            );
        }

        private static CreateProductVariantCommand ToCreateProductVariantCommand(
            CreateProductVariantRequest request
        )
        {
            return new CreateProductVariantCommand(
                Name: request.Name,
                Sku: request.Sku,
                CostPrice: request.CostPrice,
                SellingPrice: request.SellingPrice,
                Position: request.Position,
                IsArchived: request.IsArchived,
                ProductVariantImage: ToCreateProductVariantImageCommand(request.ProductVariantImage)
            );
        }

        private static CreateProductVariantImageCommand ToCreateProductVariantImageCommand(
            CreateProductVariantImageRequest request
        )
        {
            return new CreateProductVariantImageCommand(File: request.File, Alt: request.Alt);
        }
    }
}
