using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Contracts.Requests;
using SoraSora.Modules.Inventory.Mappers;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Inventory.Application.Controllers;

[ApiController]
[Route(ApiRoute.WarehouseApiRoute)]
public sealed class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    private readonly IWarehouseService _warehouseService = warehouseService;

    [HasPermission(AppPermissions.Warehouses.Create)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWarehouseRequest request,
        [FromServices] IValidator<CreateWarehouseRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult);
        }

        var command = CreateWarehouseMapper.ToCreateWarehouseCommand(request);
        var warehouse = await _warehouseService.CreateWarehouseAsync(command, cancellationToken);

        return Created($"{ApiRoute.WarehouseApiRoute}/{warehouse.Id}", warehouse);
    }

    [HasPermission(AppPermissions.Warehouses.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);

        return Ok(warehouses);
    }

    [HasPermission(AppPermissions.Warehouses.Read)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, cancellationToken);

        return Ok(warehouse);
    }

    [HasPermission(AppPermissions.Warehouses.Update)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromForm] UpdateWarehouseRequest request,
        [FromServices] IValidator<UpdateWarehouseRequest> validator,
        CancellationToken cancellationToken
    )
    {
        request = request with { Id = id };

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult);
        }

        var command = UpdateWarehouseMapper.ToUpdateWarehouseCommand(request);
        var warehouse = await _warehouseService.UpdateWarehouseAsync(command, cancellationToken);

        return Ok(warehouse);
    }

    [HasPermission(AppPermissions.Warehouses.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await _warehouseService.DeleteWarehouseByIdAsync(id, cancellationToken);

        return NoContent();
    }
}
