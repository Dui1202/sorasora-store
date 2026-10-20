using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SoraSora.Api.Application.Workflow;
using SoraSora.Api.Contracts.Requests;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Api.Application.Controllers;

[ApiController]
[Route(ApiRoute.OrderApiRoute)]
public sealed class OrderController(OrderWorkflow orderWorkflow) : ControllerBase
{
    [HasPermission(AppPermissions.Orders.PlaceOrder)]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderWorkflowRequest request,
        [FromServices] IValidator<PlaceOrderWorkflowRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, "ORDER_VALIDATION_FAILED");
        }

        var command = CreateOrderMapper.ToPlaceOrderWorkflowCommand(request);
        var order = await orderWorkflow.PlaceOrderAsync(command, cancellationToken);

        return Created($"{ApiRoute.OrderApiRoute}/{order.Id}", order);
    }

    [HasPermission(AppPermissions.Orders.Create)]
    [HttpPost("admin")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromServices] IValidator<CreateOrderRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, "ORDER_VALIDATION_FAILED");
        }

        var command = CreateOrderMapper.ToCreateOrderWorkflowCommand(request);
        var order = await orderWorkflow.CreateOrderAsync(command, cancellationToken);

        return Created($"{ApiRoute.OrderApiRoute}/{order.Id}", order);
    }
    
}
