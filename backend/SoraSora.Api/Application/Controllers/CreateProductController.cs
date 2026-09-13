using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Api.Application.Workflow;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Api.Application.Controllers;

[ApiController]
[Route(ApiRoute.ProductsApiRoute)]
public sealed class CreateProductController(CreateProductWorkflow workflow) : ControllerBase
{
    [HasPermission(AppPermissions.Products.Create)]
    [RequestSizeLimit(ProductUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProductUploadLimits.MaxRequestBytes)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductRequest request,
        [FromServices] IValidator<CreateProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }

        var command = CreateProductWorkflowMapper.ToCreateProductWorkflowCommand(request);
        var product = await workflow.CreateProductAsync(command, cancellationToken);

        return Created($"{ApiRoute.ProductsApiRoute}/{product.Id}", product);
    }
}
