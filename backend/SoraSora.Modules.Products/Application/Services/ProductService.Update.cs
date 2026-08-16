using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Media.Domain.Upload;
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
    public async Task<ProductResponse> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);
        var imageAssetIdsToRemove = new HashSet<string>(StringComparer.Ordinal);
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

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
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

                RemoveProductVariantsFromProduct(product, command, imageAssetIdsToRemove);

                var variationOptionsByVariationClientId = await ReconcileVariationGraphAsync(
                    product,
                    command,
                    cancellationToken
                );

                await UpdateProductVariantPositions(
                    product,
                    command,
                    existingProductVariantsById,
                    cancellationToken
                );

                UpdateProductVariants(
                    product,
                    command,
                    existingProductVariantsById,
                    imageAssetIdsToRemove,
                    out var updatedProductVariants,
                    out var createdProductVariants
                );

                AttachProductVariantOptions(
                    product,
                    command,
                    existingProductVariantsById,
                    createdProductVariants,
                    variationOptionsByVariationClientId
                );

                await UpdateProductImagePositionsAsync(
                    product,
                    command,
                    imageAssetIdsToRemove,
                    cancellationToken
                );

                var productImageUploadJobs = BuildProductImageUploadJobForUpdateCommand(
                    product,
                    command,
                    updatedProductVariants,
                    createdProductVariants
                );

                var mediaUploadResults = await _imageMediaService.UploadImagesAsync(
                    productImageUploadJobs.Select(productJob => productJob.Job).ToList(),
                    cancellationToken
                );

                AttachUploadedMedia(
                    productImageUploadJobs,
                    mediaUploadResults,
                    uploadedMediaAssetIds
                );

                await _imageMediaService.ScheduleImageDeletionAsync(
                    imageAssetIdsToRemove,
                    cancellationToken
                );

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
        catch (Exception ex)
        {
            await TryScheduleUploadedMediaCleanupAsync(uploadedMediaAssetIds);

            if (ex is DbUpdateConcurrencyException)
            {
                throw new ProductConcurrencyException(product.Id, ex);
            }

            if (ex is ProductDomainException domainException)
            {
                throw new ProductValidationException(domainException.Message, domainException);
            }

            throw;
        }

        _logger.ProductUpdated(product.Name, product.Id);

        return ProductDtoMapper.ToProductResponse(product);
    }

    private void RemoveProductVariantsFromProduct(
        Product product,
        UpdateProductCommand command,
        HashSet<string> imageAssetIdsToRemove
    )
    {
        var retainedProductVariantsId = command
            .RetainedProductVariants.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var productVariantsToRemove = product
            .ProductVariants.Where(x => !retainedProductVariantsId.Contains(x.Id))
            .ToList();

        //Remove non-retained product variant
        foreach (var productVariant in productVariantsToRemove)
        {
            if (productVariant.ProductVariantImage is not null)
            {
                imageAssetIdsToRemove.Add(productVariant.ProductVariantImage.MediaAssetId);
            }

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

    private void UpdateProductVariants(
        Product product,
        UpdateProductCommand command,
        IDictionary<string, ProductVariant> existingProductVariantsById,
        HashSet<string> imageAssetIdsToRemove,
        out List<ImageUpdatedProductVariant> imageUpdatedProductVariants,
        out List<CreatedProductVariant> createdProductVariants
    )
    {
        imageUpdatedProductVariants = [];
        createdProductVariants = [];
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

            //Update product variant image operation
            var newImage = productVariantCommand.NewImage;

            ///Skip if the new image command is null
            if (newImage is null)
            {
                continue;
            }

            ///Delete the original image
            if (productVariantToUpdate.ProductVariantImage is not null)
            {
                imageAssetIdsToRemove.Add(productVariantToUpdate.ProductVariantImage.MediaAssetId);
            }
            imageUpdatedProductVariants.Add(
                new ImageUpdatedProductVariant(
                    ProductVariant: productVariantToUpdate,
                    Command: productVariantCommand
                )
            );
        }

        //Add new product variant
        foreach (var newProductVariantCommand in command.NewProductVariants)
        {
            var productVariant = ProductVariant.Create(
                name: newProductVariantCommand.Name,
                sku: newProductVariantCommand.Sku,
                costPrice: newProductVariantCommand.CostPrice,
                sellingPrice: newProductVariantCommand.SellingPrice,
                position: newProductVariantCommand.Position
            );

            product.AddProductVariant(productVariant);
            createdProductVariants.Add(
                new CreatedProductVariant(
                    ProductVariant: productVariant,
                    Command: newProductVariantCommand
                )
            );
        }
    }

    private async Task<
        Dictionary<string, Dictionary<string, VariationOption>>
    > ReconcileVariationGraphAsync(
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
                _dbContext.Set<Variation>().Add(variation);
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
                    _dbContext.Set<VariationOption>().Add(option);
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
        IReadOnlyDictionary<string, ProductVariant> existingProductVariantsById,
        IReadOnlyCollection<CreatedProductVariant> createdProductVariants,
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

    private static void AttachUploadedMedia(
        IReadOnlyList<ProductImageUploadJob> productImageUploadJobs,
        IReadOnlyList<MediaUploadResult> mediaUploadResults,
        ISet<string> uploadedMediaAssetIds
    )
    {
        var attachMediaByUploadIds = productImageUploadJobs.ToDictionary(
            productJob => productJob.Job.Id,
            productJob => productJob.Attach,
            StringComparer.Ordinal
        );

        foreach (var result in mediaUploadResults)
        {
            uploadedMediaAssetIds.Add(result.AssetId);
        }

        if (mediaUploadResults.Count != productImageUploadJobs.Count)
        {
            throw new InvalidOperationException(
                "The media service did not return a result for every upload job"
            );
        }

        foreach (var result in mediaUploadResults)
        {
            if (!attachMediaByUploadIds.TryGetValue(result.JobId, out var attach))
            {
                throw new InvalidOperationException(
                    "This media upload result is not from the original upload job"
                );
            }

            attach(result.AssetId);
        }
    }
}
