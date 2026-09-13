using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Api.Application.Workflow;
using SoraSora.Api.Mappers;
using SoraSora.Modules.Orders.Contracts.Requests;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

[ApiController]
[Route(ApiRoute.OrderApiRoute)]
public sealed class OrderController(OrderWorkflow orderWorkflow) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromForm] CreateOrderRequest request,
        [FromServices] IValidator<CreateOrderRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult, "ORDER_VALIDATION_FAILED");
        }

        var command = PlaceOrderWorkflowMapper.ToPlaceOrderWorkflowCommand(request);
        var order = await orderWorkflow.PlaceOrderAsync(command, cancellationToken);

        return Created($"{ApiRoute.OrderApiRoute}/{order.Id}", order);
    }
}
