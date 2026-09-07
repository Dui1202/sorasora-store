using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

[ApiController]
[Route(ApiRoute.InventoryApiRoute)]
public sealed class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    private readonly IInventoryService _inventoryService = inventoryService;
}
