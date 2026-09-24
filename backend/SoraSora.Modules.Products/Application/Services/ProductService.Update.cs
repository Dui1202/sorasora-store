using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Application.Services.Loggers;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Modules.Products.Application.Services;

//TODO: Archive product variants instead of deleting them permanently
public sealed partial class ProductService
{
    private sealed record CreatedProductVariant
    {
        public required ProductVariant ProductVariant { get; init; }
        public required CreateProductVariantCommand Command { get; init; }
    }

    public async Task<Product> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        // var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);
        // var imageAssetIdsToRemove = new HashSet<string>(StringComparer.Ordinal);
        var product =
            await _dbContext
                .Products()
                .Where(x => !x.IsArchived)
                .AsSplitQuery()
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantImage)
                .Include(x => x.ProductVariants)
                    .ThenInclude(x => x.ProductVariantOptions)
                .Include(x => x.Variations)
                    .ThenInclude(x => x.VariationOptions)
                .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new ProductNotFoundException(command.Id);

        if (product.Version != command.Version)
        {
            throw new ProductConcurrencyException(product.Id);
        }

        product.UpdateDetails(
            command.Name,
            command.Description,
            command.SupplierCountry,
            command.SellMode,
            command.IsPublished
        );
        product.AdvanceVersion();

        var existingProductVariantsById = product.ProductVariants.ToDictionary(
            variant => variant.Id,
            StringComparer.Ordinal
        );

        var createdProductVariants = await HandleProductVariants(
            product,
            command,
            existingProductVariantsById,
            cancellationToken
        );

        //Handle variations and variation options update
        var variationOptionsByVariationClientId = await HandleVariationUpdateAsync(
            product,
            command,
            cancellationToken
        );

        AttachProductVariantOptions(
            product,
            command,
            createdProductVariants,
            existingProductVariantsById,
            variationOptionsByVariationClientId
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.ProductUpdated(product.Name, product.Id);

        return product;
    }

    private async Task<IReadOnlyCollection<CreatedProductVariant>> HandleProductVariants(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        CancellationToken cancellationToken
    )
    {
        RemoveUnretainedProductVariants(product, command);
        UpdateRetainedProductVariants(product, command, existingProductVariantsById);
        var createdProductVariants = AddNewProductVariants(product, command);
        await UpdateProductVariantPositions(
            product,
            command,
            existingProductVariantsById,
            cancellationToken
        );

        return createdProductVariants;
    }

    private void RemoveUnretainedProductVariants(Product product, UpdateProductCommand command)
    {
        var retainedProductVariantsId = command
            .RetainedProductVariants.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var productVariantsToRemove = product
            .ProductVariants.Where(x => !retainedProductVariantsId.Contains(x.Id))
            .ToList();

        foreach (var productVariant in productVariantsToRemove)
        {
            product.RemoveProductVariant(productVariant);
        }
    }

    private async Task UpdateProductVariantPositions(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        CancellationToken cancellationToken
    )
    {
        var occupiedPositions = product.ProductVariants.Select(x => x.Position).ToList();
        var finalPositions = command.RetainedProductVariants.Select(x => x.Position).ToList();
        var tempPositions = AllocateTemporaryPositions(
            occupiedPositions,
            finalPositions,
            command.RetainedProductVariants.Count
        );
        var tempIndex = 0;

        foreach (var productVariant in command.RetainedProductVariants)
        {
            if (
                !existingProductVariantsById.TryGetValue(
                    productVariant.Id,
                    out var productVariantToUpdate
                )
            )
            {
                throw new ProductVariantOwnershipException(productVariant.Id, product.Id);
            }

            productVariantToUpdate.UpdatePosition(tempPositions[tempIndex++]);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void UpdateRetainedProductVariants(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById
    )
    {
        //Update retained product variants
        foreach (var productVariantCommand in command.RetainedProductVariants)
        {
            //Update product variant details
            if (
                !existingProductVariantsById.TryGetValue(
                    productVariantCommand.Id,
                    out var productVariantToUpdate
                )
            )
            {
                throw new ProductVariantOwnershipException(productVariantCommand.Id, product.Id);
            }

            productVariantToUpdate.UpdateDetails(
                name: productVariantCommand.Name,
                sku: productVariantCommand.Sku,
                costPrice: productVariantCommand.CostPrice,
                sellingPrice: productVariantCommand.SellingPrice,
                position: productVariantCommand.Position
            );
        }
    }

    private IReadOnlyCollection<CreatedProductVariant> AddNewProductVariants(
        Product product,
        UpdateProductCommand command
    )
    {
        var createdVariants = new List<CreatedProductVariant>();
        //Add new product variant
        foreach (var newProductVariantCommand in command.NewProductVariants)
        {
            var productVariant = ProductVariant.Create(
                clientId: newProductVariantCommand.ClientId,
                name: newProductVariantCommand.Name,
                sku: newProductVariantCommand.Sku,
                costPrice: newProductVariantCommand.CostPrice,
                sellingPrice: newProductVariantCommand.SellingPrice,
                position: newProductVariantCommand.Position
            );

            product.AddProductVariant(productVariant);

            createdVariants.Add(
                new CreatedProductVariant
                {
                    ProductVariant = productVariant,
                    Command = newProductVariantCommand,
                }
            );
        }
        return createdVariants;
    }

    private async Task<
        Dictionary<string, Dictionary<string, VariationOption>>
    > HandleVariationUpdateAsync(
        Product product,
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingVariationsById = product.Variations.ToDictionary(
            variation => variation.Id,
            StringComparer.Ordinal
        );

        ValidateVariationOwnership(product, command, existingVariationsById);

        //Remove all product variant options
        foreach (var productVariant in product.ProductVariants)
        {
            foreach (var selection in productVariant.ProductVariantOptions.ToList())
            {
                productVariant.RemoveProductVariantOption(selection);
            }
        }

        var retainedVariationIds = command
            .Variations.Where(x => x.Id is not null)
            .Select(x => x.Id!)
            .ToHashSet(StringComparer.Ordinal);

        // Remove unretained variations
        foreach (
            var variation in product
                .Variations.Where(x => !retainedVariationIds.Contains(x.Id))
                .ToList()
        )
        {
            product.RemoveVariation(variation);
        }

        //Updating existing variations
        foreach (var variationCommand in command.Variations.Where(x => x.Id is not null))
        {
            var variation = existingVariationsById[variationCommand.Id!];
            var retainedOptionIds = variationCommand
                .VariationOptions.Where(x => x.Id is not null)
                .Select(x => x.Id!)
                .ToHashSet(StringComparer.Ordinal);

            //Remove unretained variation options
            foreach (
                var option in variation
                    .VariationOptions.Where(x => !retainedOptionIds.Contains(x.Id))
                    .ToList()
            )
            {
                variation.RemoveOption(option);
            }

            variation.UpdateDetails(CreateTemporaryUniqueValue("variation"), variation.Position);
            foreach (var option in variation.VariationOptions)
            {
                option.UpdateDetails(CreateTemporaryUniqueValue("option"), option.Position);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var optionsByVariationClientId = new Dictionary<
            string,
            Dictionary<string, VariationOption>
        >(StringComparer.Ordinal);

        foreach (var variationCommand in command.Variations)
        {
            var variation = variationCommand.Id is null
                ? Variation.Create(variationCommand.Name, variationCommand.Position)
                : existingVariationsById[variationCommand.Id];

            if (variationCommand.Id is null)
            {
                product.AddVariation(variation);
                _dbContext.Variations().Add(variation);
            }
            else
            {
                variation.UpdateDetails(variationCommand.Name, variationCommand.Position);
            }

            var existingOptionsById = variation.VariationOptions.ToDictionary(
                option => option.Id,
                StringComparer.Ordinal
            );
            var optionsByClientId = new Dictionary<string, VariationOption>(StringComparer.Ordinal);

            foreach (var optionCommand in variationCommand.VariationOptions)
            {
                var option = optionCommand.Id is null
                    ? VariationOption.Create(optionCommand.Value, optionCommand.Position)
                    : existingOptionsById[optionCommand.Id];

                if (optionCommand.Id is null)
                {
                    variation.AddOption(option);
                    _dbContext.VariationOptions().Add(option);
                }
                else
                {
                    option.UpdateDetails(optionCommand.Value, optionCommand.Position);
                }

                optionsByClientId.Add(optionCommand.ClientId, option);
            }

            optionsByVariationClientId.Add(variationCommand.ClientId, optionsByClientId);
        }

        return optionsByVariationClientId;
    }

    private static void ValidateVariationOwnership(
        Product product,
        UpdateProductCommand command,
        IReadOnlyDictionary<string, Variation> existingVariationsById
    )
    {
        foreach (var variationCommand in command.Variations)
        {
            Variation? variation = null;
            if (
                variationCommand.Id is not null
                && !existingVariationsById.TryGetValue(variationCommand.Id, out variation)
            )
            {
                throw new ProductVariationOwnershipException(variationCommand.Id, product.Id);
            }

            foreach (var optionCommand in variationCommand.VariationOptions)
            {
                if (optionCommand.Id is null)
                {
                    continue;
                }

                if (
                    variation is null
                    || variation.VariationOptions.All(x => x.Id != optionCommand.Id)
                )
                {
                    throw new ProductVariationOptionOwnershipException(
                        optionCommand.Id,
                        variationCommand.Id ?? variationCommand.ClientId
                    );
                }
            }
        }
    }

    private static void AttachProductVariantOptions(
        Product product,
        UpdateProductCommand command,
        IReadOnlyCollection<CreatedProductVariant> createdProductVariants,
        IReadOnlyDictionary<string, ProductVariant> existingProductVariantsById,
        IReadOnlyDictionary<string, Dictionary<string, VariationOption>> optionsByVariationClientId
    )
    {
        foreach (var variantCommand in command.RetainedProductVariants)
        {
            if (!existingProductVariantsById.TryGetValue(variantCommand.Id, out var productVariant))
            {
                throw new ProductVariantOwnershipException(variantCommand.Id, product.Id);
            }

            AttachProductVariantOptions(
                product,
                productVariant,
                variantCommand.ProductVariantOptions,
                optionsByVariationClientId
            );
        }

        foreach (var createdProductVariant in createdProductVariants)
        {
            AttachProductVariantOptions(
                product,
                createdProductVariant.ProductVariant,
                createdProductVariant.Command.ProductVariantOptions,
                optionsByVariationClientId
            );
        }
    }

    private static void AttachProductVariantOptions(
        Product product,
        ProductVariant productVariant,
        IReadOnlyCollection<CreateProductVariantOptionCommand> selections,
        IReadOnlyDictionary<string, Dictionary<string, VariationOption>> optionsByVariationClientId
    )
    {
        foreach (var selection in selections)
        {
            if (
                !optionsByVariationClientId.TryGetValue(
                    selection.VariationClientId,
                    out var optionsByClientId
                )
            )
            {
                throw new ProductVariationOwnershipException(
                    selection.VariationClientId,
                    product.Id
                );
            }

            if (
                !optionsByClientId.TryGetValue(
                    selection.VariationOptionClientId,
                    out var variationOption
                )
            )
            {
                throw new ProductVariationOptionOwnershipException(
                    selection.VariationOptionClientId,
                    selection.VariationClientId
                );
            }

            productVariant.AddProductVariantOption(
                ProductVariantOption.Create(productVariant, variationOption)
            );
        }
    }

    private static string CreateTemporaryUniqueValue(string prefix) =>
        $"__{prefix}_update_{Guid.NewGuid():N}";

    private async Task UpdateProductImagePositionsAsync(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imageAssetIdsToRemove,
        CancellationToken cancellationToken
    )
    {
        var existingProductImagesByAssetId = product.ProductImages.ToDictionary(
            x => x.MediaAssetId,
            StringComparer.Ordinal
        );
        var retainedProductImageAssetIds = command
            .RetainedImages.Select(x => x.MediaAssetId)
            .ToHashSet(StringComparer.Ordinal);

        var productImagesToRemove = product
            .ProductImages.Where(x => !retainedProductImageAssetIds.Contains(x.MediaAssetId))
            .ToList();
        //Remove non-retained product images
        foreach (var image in productImagesToRemove)
        {
            imageAssetIdsToRemove.Add(image.MediaAssetId);

            product.RemoveImageByAssetId(image.MediaAssetId);
        }

        //Apply temporary positions
        var occupiedPositions = product.ProductImages.Select(x => x.Position).ToList();
        var finalPositions = command.RetainedImages.Select(x => x.Position).ToList();
        var tempPositions = AllocateTemporaryPositions(
            occupiedPositions,
            finalPositions,
            command.RetainedImages.Count
        );
        var tempIndex = 0;

        foreach (var image in command.RetainedImages)
        {
            if (
                !existingProductImagesByAssetId.TryGetValue(
                    image.MediaAssetId,
                    out var productImage
                )
            )
            {
                throw new ProductImageOwnershipException(image.MediaAssetId, product.Id);
            }

            productImage.UpdateDetails(position: tempPositions[tempIndex++]);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        //Update product images
        var altTextByMediaAssetId = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var image in command.RetainedImages)
        {
            if (
                !existingProductImagesByAssetId.TryGetValue(
                    image.MediaAssetId,
                    out var productImage
                )
            )
            {
                throw new ProductImageOwnershipException(image.MediaAssetId, product.Id);
            }

            productImage.UpdateDetails(position: image.Position);
            altTextByMediaAssetId.Add(image.MediaAssetId, image.Alt);
        }

        await _imageMediaService.UpdateImageAltTextsAsync(altTextByMediaAssetId, cancellationToken);
    }

    private static List<int> AllocateTemporaryPositions(
        IEnumerable<int> occupiedPositions,
        IEnumerable<int> finalPositions,
        int count
    )
    {
        var unavailablePositions = occupiedPositions.Concat(finalPositions).ToHashSet();

        var temporaryPositions = new List<int>(count);

        for (var candidate = int.MaxValue; temporaryPositions.Count < count; candidate--)
        {
            if (unavailablePositions.Add(candidate))
            {
                temporaryPositions.Add(candidate);
            }
        }

        return temporaryPositions;
    }
}
