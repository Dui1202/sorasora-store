using System.Data;
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

public sealed partial class ProductService
{
    private sealed record CreatedProductVariant
    {
        public required ProductVariant ProductVariant { get; init; }
        public required CreateProductVariantCommand Command { get; init; }
    }

    public async Task<Product> UpdateProductAsync(
        Product product,
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        if (product.Version != command.Version)
        {
            throw new ProductConcurrencyException(product.Id);
        }

        var ownTransaction = _dbContext.Database.CurrentTransaction is null;
        await using var transaction = ownTransaction
            ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
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

            var createdProductVariants = await HandleProductVariantsUpdateAsync(
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

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception exception)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            if (exception is DbUpdateConcurrencyException)
            {
                throw new ProductConcurrencyException(product.Id, exception);
            }

            throw;
        }

        _logger.ProductUpdated(product.Name, product.Id);

        return product;
    }

    private async Task<IReadOnlyCollection<CreatedProductVariant>> HandleProductVariantsUpdateAsync(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        CancellationToken cancellationToken
    )
    {
        ArchiveUnretainedProductVariants(product, command);
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

    private void ArchiveUnretainedProductVariants(Product product, UpdateProductCommand command)
    {
        var retainedProductVariantsId = command
            .RetainedProductVariants.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var productVariantsToRemove = product
            .ProductVariants.Where(x => !retainedProductVariantsId.Contains(x.Id))
            .ToList();

        foreach (var productVariant in productVariantsToRemove)
        {
            productVariant.Archive();
        }

        var variantImages = productVariantsToRemove
            .Select(x => x.ProductVariantImage)
            .OfType<ProductVariantImage>()
            .ToList()
            .ToList();

        _dbContext.RemoveRange(variantImages);
    }

    private async Task UpdateProductVariantPositions(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        CancellationToken cancellationToken
    )
    {
        var occupiedPositions = product
            .ProductVariants.Select(x => x.Position)
            .OfType<int>()
            .ToList();
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

            productVariantToUpdate.UpdatePosition(productVariant.Position);
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

        var occupiedVariationPositions = product
            .Variations.Select(x => x.Position)
            .OfType<int>()
            .ToList();
        var finalVariationPositions = command.Variations.Select(x => x.Position).ToList();
        var tempVariationPositions = AllocateTemporaryPositions(
            occupiedVariationPositions,
            finalVariationPositions,
            command.Variations.Count
        );
        var tempVariationIndex = 0;
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

            variation.UpdateDetails(
                CreateTemporaryUniqueValue("variation"),
                tempVariationPositions[tempVariationIndex++]
            );

            var occupiedOptionPositions = variation
                .VariationOptions.Select(x => x.Position)
                .OfType<int>()
                .ToList();
            var finalOptionPositions = variationCommand
                .VariationOptions.Select(x => x.Position)
                .ToList();
            var tempOptionPositions = AllocateTemporaryPositions(
                occupiedOptionPositions,
                finalOptionPositions,
                variationCommand.VariationOptions.Count
            );
            var tempOptionIndex = 0;

            foreach (var option in variation.VariationOptions)
            {
                option.UpdateDetails(
                    CreateTemporaryUniqueValue("option"),
                    tempOptionPositions[tempOptionIndex++]
                );
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
