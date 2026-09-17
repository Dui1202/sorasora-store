using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.S3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using SoraSora.Api.Application.Workflow;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Mappers;
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
        [FromForm] CreateProductWorkflowRequest request,
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

        MetadataValidation(payload, files);

        var variantImageFilesByClientId = files
            .Where(file => file.Name.StartsWith(VariantImageFieldPrefix, StringComparison.Ordinal))
            .ToDictionary(
                file => file.Name[VariantImageFieldPrefix.Length..],
                file => file,
                StringComparer.Ordinal
            );

        var productImageFilesByClientId = files
            .Where(file => file.Name.StartsWith(ProductImageFieldPrefix, StringComparison.Ordinal))
            .ToDictionary(
                file => file.Name[ProductImageFieldPrefix.Length..],
                file => file,
                StringComparer.Ordinal
            );

        var createProductCommand = CreateProductWorkflowMapper.ToCreateProductWorkflowCommand(
            payload,
            productImageFilesByClientId,
            variantImageFilesByClientId
        );

        var product = await workflow.CreateProductAsync(createProductCommand, cancellationToken);

        return Created($"{ApiRoute.ProductsApiRoute}/{product.Id}", product);
    }

    private static void MetadataValidation(CreateProductRequest payload, IFormFileCollection files)
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

        var productVariantImagesMetadata = payload
            .ProductVariants.Select(x => x.ProductVariantImage)
            .ToList();

        var imageCount = payload.ProductImages.Count + productVariantImagesMetadata.Count;

        if (files.Count != imageCount)
        {
            throw new BadRequestException(
                "The file count is not equal to the images metadata count."
            );
        }
    }
}
