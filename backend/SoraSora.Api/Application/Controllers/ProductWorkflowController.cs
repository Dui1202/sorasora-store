using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SoraSora.Api.Application.Workflow.Products;
using SoraSora.Api.Contracts.Requests;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Api.Contracts.Requests.Products.Update;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Shared.Exceptions;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Api.Application.Controllers;

[ApiController]
[Route(ApiRoute.ProductsApiRoute)]
public sealed class ProductWorkflowController(
    ProductWorkflow workflow,
    IOptions<JsonOptions> jsonOptions
) : ControllerBase
{
    private const string VariantImageFieldPrefix = "variantImage:";
    private const string ProductImageFieldPrefix = "productImage:";

    [HasPermission(AppPermissions.Products.Create)]
    [RequestSizeLimit(ProductUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProductUploadLimits.MaxRequestBytes)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] PayloadRequest request,
        [FromServices] IValidator<CreateProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var payload =
            JsonSerializer.Deserialize<CreateProductRequest>(
                request.Payload,
                jsonOptions.Value.JsonSerializerOptions
            ) ?? throw new BadRequestException("The payload is empty");

        var validationResult = await validator.ValidateAsync(payload, cancellationToken);

        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }

        var files = Request.Form.Files;

        var (productImageFilesByClientId, variantImageFilesByClientId) = BuildImageFilesDictionary(
            files
        );

        MetadataValidationForCreate(
            payload,
            files,
            productImageFilesByClientId,
            variantImageFilesByClientId
        );

        var createProductCommand = CreateProductWorkflowMapper.ToCreateProductWorkflowCommand(
            payload,
            productImageFilesByClientId,
            variantImageFilesByClientId
        );

        var product = await workflow.CreateProductAsync(createProductCommand, cancellationToken);

        return Created($"{ApiRoute.ProductsApiRoute}/{product.Id}", product);
    }

    [HasPermission(AppPermissions.Products.Update)]
    [RequestSizeLimit(ProductUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProductUploadLimits.MaxRequestBytes)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromForm] PayloadRequest request,
        [FromServices] IValidator<UpdateProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var payload =
            JsonSerializer.Deserialize<UpdateProductRequest>(
                request.Payload,
                jsonOptions.Value.JsonSerializerOptions
            ) ?? throw new BadRequestException("The payload is empty");

        payload = payload with { Id = id };

        var validationResult = await validator.ValidateAsync(payload, cancellationToken);

        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }

        var files = Request.Form.Files;

        var (productImageFilesByClientId, variantImageFilesByClientId) = BuildImageFilesDictionary(
            files
        );

        MetadataValidationForUpdate(
            payload,
            files,
            productImageFilesByClientId,
            variantImageFilesByClientId
        );

        var command = UpdateProductWorkflowMapper.ToUpdateProductWorkflowCommand(
            payload,
            productImageFilesByClientId,
            variantImageFilesByClientId
        );

        var product = await workflow.UpdateProductAsync(command, cancellationToken);

        return Ok(product);
    }

    [HasPermission(AppPermissions.Products.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Archive(
        string id,
        ArchiveProductRequest request,
        IValidator<ArchiveProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        request = request with { ProductId = id };
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }

        await workflow.ArchiveProductAsync(request.ProductId, request.Version, cancellationToken);

        return NoContent();
    }

    private static void MetadataValidationForCreate(
        CreateProductRequest payload,
        IFormFileCollection files,
        IDictionary<string, IFormFile> productImages,
        IDictionary<string, IFormFile> variantImages
    )
    {
        if (files.Count == 0)
        {
            throw new BadRequestException("There is no file in this request");
        }

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                throw new BadRequestException("Image file cannot be empty.");
            }

            if (file.Length > ProductUploadLimits.MaxImageBytes)
            {
                throw new BadRequestException(
                    $"Image file cannot exceed {ProductUploadLimits.MaxImageBytes / (1024 * 1024)} MB."
                );
            }
        }

        var expectedProductImageIds = payload
            .ProductImages.Select(x => x.ClientId)
            .ToHashSet(StringComparer.Ordinal);

        var expectedVariantImageIds = payload
            .ProductVariants.Where(x => x.ProductVariantImage is not null)
            .Select(x => x.ProductVariantImage!.ClientId)
            .ToHashSet(StringComparer.Ordinal);

        if (!expectedProductImageIds.SetEquals(productImages.Keys))
        {
            throw new BadRequestException(
                "Product image files do not match product image metadata."
            );
        }

        if (!expectedVariantImageIds.SetEquals(variantImages.Keys))
        {
            throw new BadRequestException(
                "Variant image files do not match variant image metadata."
            );
        }
    }

    private static void MetadataValidationForUpdate(
        UpdateProductRequest payload,
        IFormFileCollection files,
        IDictionary<string, IFormFile> productImages,
        IDictionary<string, IFormFile> variantImages
    )
    {
        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                throw new BadRequestException("Image file cannot be empty.");
            }

            if (file.Length > ProductUploadLimits.MaxImageBytes)
            {
                throw new BadRequestException(
                    $"Image file cannot exceed {ProductUploadLimits.MaxImageBytes / (1024 * 1024)} MB."
                );
            }
        }

        var expectedProductImageIds = payload
            .NewImages.Select(x => x.ClientId)
            .ToHashSet(StringComparer.Ordinal);

        var expectedImageIdFromNewVariants = payload.NewProductVariants.Select(x =>
            x.ProductVariantImage.ClientId
        );

        var expectedImageIdFromRetainedVariants = payload
            .RetainedProductVariants.Where(x => x.NewImage is not null)
            .Select(x => x.NewImage!.ClientId);

        var expectedVariantImageIds = expectedImageIdFromNewVariants
            .Concat(expectedImageIdFromRetainedVariants)
            .ToHashSet(StringComparer.Ordinal);

        if (!expectedProductImageIds.SetEquals(productImages.Keys))
        {
            throw new BadRequestException(
                "Product image files do not match product image metadata."
            );
        }

        if (!expectedVariantImageIds.SetEquals(variantImages.Keys))
        {
            throw new BadRequestException(
                "Variant image files do not match variant image metadata."
            );
        }
    }

    private static (
        IDictionary<string, IFormFile> ProductImages,
        IDictionary<string, IFormFile> VariantImages
    ) BuildImageFilesDictionary(IFormFileCollection files)
    {
        var productImageFilesByClientId = new Dictionary<string, IFormFile>(StringComparer.Ordinal);

        var variantImageFilesByClientId = new Dictionary<string, IFormFile>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            Dictionary<string, IFormFile> target;
            string clientId;

            if (file.Name.StartsWith(ProductImageFieldPrefix, StringComparison.Ordinal))
            {
                clientId = file.Name[ProductImageFieldPrefix.Length..];
                target = productImageFilesByClientId;
            }
            else if (file.Name.StartsWith(VariantImageFieldPrefix, StringComparison.Ordinal))
            {
                clientId = file.Name[VariantImageFieldPrefix.Length..];
                target = variantImageFilesByClientId;
            }
            else
            {
                throw new BadRequestException($"Unknown file field '{file.Name}'.");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new BadRequestException("Image file client ID cannot be empty.");
            }

            if (!target.TryAdd(clientId, file))
            {
                throw new BadRequestException($"Duplicate image client ID '{clientId}'.");
            }
        }

        return (productImageFilesByClientId, variantImageFilesByClientId);
    }
}
