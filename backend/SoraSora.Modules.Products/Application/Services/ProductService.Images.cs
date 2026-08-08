using Microsoft.AspNetCore.Http;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Storage;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    private static IReadOnlyList<ProductImageUploadJob> BuildImageUploadJob(
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
                    imageUploadJob,
                    path => product.AddImage(path, image.Alt, image.Position)
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
                    imageUploadJob,
                    path =>
                        variant.SetProductVariantImage(
                            ProductVariantImage.Create(path, variantCommand.ProductVariantImage.Alt)
                        )
                )
            );
        }
    }

    private sealed record CreatedProductVariant(
        ProductVariant ProductVariant,
        CreateProductVariantCommand Command
    );

    private sealed record ProductImageUploadJob(ImageUploadJob Job, Action<string> Attach);
}
