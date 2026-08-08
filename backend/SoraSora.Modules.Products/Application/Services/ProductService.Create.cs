using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    public async Task<ProductResponse> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        Product product;

        try
        {
            product = Product.Create(
                name: command.Name,
                description: command.Description,
                supplierCountry: command.SupplierCountry,
                sellMode: command.SellMode,
                isPublished: command.IsPublished,
                hasVariation: command.HasVariation
            );
        }
        catch (ProductDomainException exception)
        {
            throw new ProductValidationException(exception.Message, exception);
        }

        try
        {
            CreateVariations(product, command, out var variationOptionsByVariationClientId);
            await CreateProductVariantsAsync(
                product,
                command,
                variationOptionsByVariationClientId,
                cancellationToken
            );
            await CreateProductImagesAsync(product, command, cancellationToken);

            _dbContext.Products().Add(product);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is ProductDomainException domainException)
            {
                throw new ProductValidationException(domainException.Message, domainException);
            }

            throw;
        }

        _logger.ProductCreated(product.Name, product.Id);

        return ProductDtoMapper.ToProductResponse(product);
    }

    private static void CreateVariations(
        Product product,
        CreateProductCommand command,
        out Dictionary<
            string,
            Dictionary<string, VariationOption>
        > variationOptionsByVariationClientId
    )
    {
        variationOptionsByVariationClientId = new Dictionary<
            string,
            Dictionary<string, VariationOption>
        >(StringComparer.Ordinal);

        foreach (var variationCommand in command.Variations)
        {
            var variation = Variation.Create(variationCommand.Name, variationCommand.Position);
            var variationOptionsByClientId = new Dictionary<string, VariationOption>();
            foreach (var optionCommand in variationCommand.VariationOptions)
            {
                var option = VariationOption.Create(optionCommand.Value, optionCommand.Position);
                variationOptionsByClientId.TryAdd(optionCommand.ClientId, option);
                variation.AddOption(option);
            }

            if (
                !variationOptionsByVariationClientId.TryAdd(
                    variationCommand.ClientId,
                    variationOptionsByClientId
                )
            )
                product.AddVariation(variation);
        }
    }

    private async Task CreateProductVariantsAsync(
        Product product,
        CreateProductCommand command,
        Dictionary<string, Dictionary<string, VariationOption>> variationOptionsByVariationClientId,
        CancellationToken cancellationToken
    )
    {
        var variationsByClientId = command.Variations.ToDictionary(x => x.ClientId);
        foreach (var productVariantCommand in command.ProductVariants)
        {
            var productVariant = ProductVariant.Create(
                name: productVariantCommand.Name,
                sku: productVariantCommand.Sku,
                costPrice: productVariantCommand.CostPrice,
                sellingPrice: productVariantCommand.SellingPrice,
                position: productVariantCommand.Position
            );

            foreach (var productVariantOptionCommand in productVariantCommand.ProductVariantOptions)
            {
                var variationClientId = productVariantOptionCommand.VariationClientId;

                if (
                    !variationOptionsByVariationClientId.TryGetValue(
                        variationClientId,
                        out var variationOptionsByClientId
                    )
                )
                {
                    throw new ProductVariationOwnershipException(variationClientId, product.Id);
                }
                var variationOptionClientId = productVariantOptionCommand.VariationOptionClientId;
                if (
                    !variationOptionsByClientId.TryGetValue(variationOptionClientId, out var option)
                )
                {
                    throw new ProductVariationOptionOwnershipException(
                        variationOptionClientId,
                        variationClientId
                    );
                }

                var productVariantOption = ProductVariantOption.Create(productVariant, option);
                productVariant.AddProductVariantOption(productVariantOption);
            }

            var file = productVariantCommand.ProductVariantImage.File;
            var alt = productVariantCommand.ProductVariantImage.Alt;

            // productVariant.SetProductVariantImage(ProductVariantImage.Create(path, alt));

            product.AddProductVariant(productVariant);
        }
    }

    private async Task CreateProductImagesAsync(
        Product product,
        CreateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        foreach (var imageRequest in command.ProductImages)
        {
            var file = imageRequest.File;

            var alt = imageRequest.Alt;
            var position = imageRequest.Position;

            // product.AddImage(path, alt, position);
        }
    }
}
