using System.IO.Compression;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SoraSora.Api.Contracts.Commands.ProductWorkflow;
using SoraSora.Modules.Media.Contracts.Upload;
using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow
{
    private sealed record ProductImageUploadJob(ImageUploadJob Job, Action<string> Attach);

    private static IReadOnlyList<ProductImageUploadJob> BuildProductImageUploadJobsForCreateCommand(
        CreateProductWorkflowCommand command,
        Product product
    )
    {
        var jobs = new List<ProductImageUploadJob>();

        foreach (var image in command.ProductImages)
        {
            jobs.Add(
                new ProductImageUploadJob(
                    Job: new ImageUploadJob(
                        file: image.File,
                        target: new MediaStorageTarget.ProductImage(product.Id),
                        alt: image.Alt
                    ),
                    Attach: mediaAssetId =>
                        product.AddImage(ProductImage.Create(mediaAssetId, image.Position))
                )
            );
        }

        var variantsByClientId = product.ProductVariants.ToDictionary(
            variant => variant.ClientId,
            StringComparer.Ordinal
        );

        foreach (var variantCommand in command.ProductVariants)
        {
            if (!variantsByClientId.TryGetValue(variantCommand.ClientId, out var variant))
            {
                throw new InvalidOperationException(
                    "The created product variant does not match a workflow variant."
                );
            }

            var image = variantCommand.ProductVariantImage;

            jobs.Add(
                new ProductImageUploadJob(
                    Job: new ImageUploadJob(
                        file: image.File,
                        target: new MediaStorageTarget.ProductVariantImage(product.Id, variant.Id),
                        alt: image.Alt
                    ),
                    Attach: mediaAssetId =>
                        variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
                )
            );
        }

        return jobs;
    }

    private static IReadOnlyCollection<ProductImageUploadJob> BuildProductImageUploadJobsForUpdateCommand(
        UpdateProductWorkflowCommand command,
        Product product
    )
    {
        var productImageUploadJobs = new List<ProductImageUploadJob>();

        foreach (var newProductImageCommand in command.NewImages)
        {
            var job = new ImageUploadJob(
                newProductImageCommand.File,
                new MediaStorageTarget.ProductImage(product.Id),
                newProductImageCommand.Alt
            );

            var productImageJob = new ProductImageUploadJob(
                Job: job,
                Attach: (mediaAssetId) =>
                    product.AddImage(
                        ProductImage.Create(mediaAssetId, newProductImageCommand.Position)
                    )
            );

            productImageUploadJobs.Add(productImageJob);
        }

        var existingVariantsByClientId = product.ProductVariants.ToDictionary(
            x => x.ClientId,
            StringComparer.Ordinal
        );

        var existingVariantsById = product.ProductVariants.ToDictionary(
            x => x.Id,
            StringComparer.Ordinal
        );

        foreach (var newVariant in command.NewProductVariants)
        {
            if (!existingVariantsByClientId.TryGetValue(newVariant.ClientId, out var variant))
            {
                throw new InvalidOperationException(
                    $"The created variant does not match with the workflow variant."
                );
            }
            var image = newVariant.ProductVariantImage;

            var job = new ImageUploadJob(
                image.File,
                new MediaStorageTarget.ProductVariantImage(product.Id, variant.Id),
                image.Alt
            );

            var productImageJob = new ProductImageUploadJob(
                Job: job,
                Attach: (mediaAssetId) =>
                    variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
            );

            productImageUploadJobs.Add(productImageJob);
        }

        foreach (var retainedVariant in command.RetainedProductVariants)
        {
            if (retainedVariant.NewImage is not null)
            {
                if (!existingVariantsById.TryGetValue(retainedVariant.Id, out var variant))
                {
                    throw new InvalidOperationException(
                        "The created variant does not match with the workflow variant."
                    );
                }

                var image = retainedVariant.NewImage;

                var job = new ImageUploadJob(
                    image.File,
                    new MediaStorageTarget.ProductVariantImage(product.Id, variant.Id),
                    image.Alt
                );

                var productImageJob = new ProductImageUploadJob(
                    Job: job,
                    Attach: (mediaAssetId) =>
                        variant.SetProductVariantImage(ProductVariantImage.Create(mediaAssetId))
                );

                productImageUploadJobs.Add(productImageJob);
            }
        }

        return productImageUploadJobs;
    }

    private static void AttachUploadedMedia(
        IReadOnlyCollection<ProductImageUploadJob> productImageUploadJobs,
        IReadOnlyCollection<MediaUploadResult> mediaUploadResults,
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
            if (!attachMediaByUploadIds.Remove(result.JobId, out var attach))
            {
                throw new InvalidOperationException(
                    "This media upload result is not from the original upload job"
                );
            }

            attach(result.AssetId);
        }
    }
}
