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
                isPublished: command.IsPublished
            );
        }
        catch (ProductDomainException exception)
        {
            throw new ProductValidationException(exception.Message, exception);
        }

        var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            CreateVariations(product, command, out var variationOptionsByVariationClientId);

            var createdProductVariants = CreateProductVariantsAsync(
                product,
                command,
                variationOptionsByVariationClientId
            );

            //Create ProductImageUploadJobs
            var productImageUploadJobs = BuildProductImageUploadJobForCreateCommand(
                product,
                command,
                createdProductVariants
            );

            await HandleProductMediaAsset(
                productImageUploadJobs,
                uploadedMediaAssetIds,
                cancellationToken
            );

            _dbContext.Products().Add(product);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            await TryScheduleUploadedMediaCleanupAsync(uploadedMediaAssetIds);

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
                if (!variationOptionsByClientId.TryAdd(optionCommand.ClientId, option))
                {
                    throw new InvalidOperationException(
                        $"Duplicate variation option Id detected: {optionCommand.ClientId}"
                    );
                }
                variation.AddOption(option);
            }

            if (
                !variationOptionsByVariationClientId.TryAdd(
                    variationCommand.ClientId,
                    variationOptionsByClientId
                )
            )
            {
                throw new InvalidOperationException(
                    $"Duplicate variation client Id detected: {variationOptionsByClientId}"
                );
            }

            product.AddVariation(variation);
        }
    }

    private IReadOnlyList<CreatedProductVariant> CreateProductVariantsAsync(
        Product product,
        CreateProductCommand command,
        Dictionary<string, Dictionary<string, VariationOption>> variationOptionsByVariationClientId
    )
    {
        List<CreatedProductVariant> createdProductVariants = [];

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

            product.AddProductVariant(productVariant);

            createdProductVariants.Add(
                new CreatedProductVariant(
                    ProductVariant: productVariant,
                    Command: productVariantCommand
                )
            );
        }

        return createdProductVariants;
    }

    private async Task HandleProductMediaAsset(
        IReadOnlyList<ProductImageUploadJob> productImageUploadJobs,
        ISet<string> uploadedMediaAssetIds,
        CancellationToken cancellationToken
    )
    {
        var imageUploadJobs = productImageUploadJobs.Select(x => x.Job).ToList();

        var mediaUploadResults = await _imageMediaService.UploadImagesAsync(
            imageUploadJobs,
            cancellationToken
        );

        AttachUploadedMedia(
            productImageUploadJobs,
            mediaUploadResults,
            uploadedMediaAssetIds
        );
    }
}
