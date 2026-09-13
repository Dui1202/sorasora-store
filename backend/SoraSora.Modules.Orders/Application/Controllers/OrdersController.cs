using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Orders.Application.Services;
using SoraSora.Shared.Route;

[ApiController]
[Route(ApiRoute.OrderApiRoute)]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllOrdersAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);

        return Ok(order);
    }
}
