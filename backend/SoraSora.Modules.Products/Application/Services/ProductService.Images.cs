using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Storage;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    private static IReadOnlyList<ProductImageUploadJob> BuildProductImageUploadJobForCreateCommand(
        Product product,
        CreateProductCommand command,
        IReadOnlyList<CreatedProductVariant> createdProductVariants
    )
    {
        var jobs = new List<ProductImageUploadJob>(
            createdProductVariants.Count + command.ProductImages.Count
        );

        foreach (var image in command.ProductImages)
        {
            var imageUploadJob = new ImageUploadJob(
                File: image.File,
                BuildPath: contentType =>
                    ProductImagePathBuilder.BuildProductImagePath(product.Id, contentType)
            );

            jobs.Add(
                new ProductImageUploadJob(
                    Job: imageUploadJob,
                    Attach: mediaAssetId =>
                        product.AddImage(ProductImage.Create(mediaAssetId, image.Position))
                )
            );
        }

        foreach (var createdVariant in createdProductVariants)
        {
            var variant = createdVariant.ProductVariant;
            var variantCommand = createdVariant.Command;

            var imageUploadJob = new ImageUploadJob(
                File: variantCommand.ProductVariantImage.File,
                BuildPath: contentType =>
                    ProductImagePathBuilder.BuildProductVariantImagePath(
                        product.Id,
                        variant.Id,
                        contentType
                    )
            );

            jobs.Add(
                new ProductImageUploadJob(
                    Job: imageUploadJob,
                    Attach: mediaAssetId =>
                        variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
                )
            );
        }

        return jobs;
    }

    private static IReadOnlyList<ProductImageUploadJob> BuildProductImageUploadJobForUpdateCommand(
        Product product,
        UpdateProductCommand command,
        IReadOnlyList<ImageUpdatedProductVariant> updatedProductVariants,
        IReadOnlyList<CreatedProductVariant> createdProductVariants
    )
    {
        var productImageJobs = new List<ProductImageUploadJob>(
            command.NewImages.Count + updatedProductVariants.Count + createdProductVariants.Count
        );

        foreach (var newImage in command.NewImages)
        {
            var job = new ImageUploadJob(
                File: newImage.File,
                BuildPath: contentType =>
                    ProductImagePathBuilder.BuildProductImagePath(product.Id, contentType)
            );

            productImageJobs.Add(
                new ProductImageUploadJob(
                    Job: job,
                    Attach: mediaAssetId =>
                        product.AddImage(ProductImage.Create(mediaAssetId, newImage.Position))
                )
            );
        }

        foreach (var updatedVariant in updatedProductVariants)
        {
            var variant = updatedVariant.ProductVariant;
            var variantCommand = updatedVariant.Command;

            var job = new ImageUploadJob(
                File: variantCommand.NewImage!.File,
                BuildPath: contentType =>
                    ProductImagePathBuilder.BuildProductVariantImagePath(
                        product.Id,
                        variant.Id,
                        contentType
                    )
            );

            productImageJobs.Add(
                new ProductImageUploadJob(
                    Job: job,
                    Attach: mediaAssetId =>
                        variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
                )
            );
        }

        foreach (var createdVariant in createdProductVariants)
        {
            var variant = createdVariant.ProductVariant;
            var variantCommand = createdVariant.Command;

            var imageUploadJob = new ImageUploadJob(
                File: variantCommand.ProductVariantImage.File,
                BuildPath: contentType =>
                    ProductImagePathBuilder.BuildProductVariantImagePath(
                        product.Id,
                        variant.Id,
                        contentType
                    )
            );

            productImageJobs.Add(
                new ProductImageUploadJob(
                    Job: imageUploadJob,
                    Attach: mediaAssetId =>
                        variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
                )
            );
        }

        return productImageJobs;
    }

    private sealed record CreatedProductVariant(
        ProductVariant ProductVariant,
        CreateProductVariantCommand Command
    );

    private sealed record ImageUpdatedProductVariant(
        ProductVariant ProductVariant,
        RetainedProductVariantCommand Command
    );

    private sealed record ProductImageUploadJob(ImageUploadJob Job, Action<string> Attach);
}
