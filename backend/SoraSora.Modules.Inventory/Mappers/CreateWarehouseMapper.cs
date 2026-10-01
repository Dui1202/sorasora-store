using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Inventory.Contracts.Requests;

namespace SoraSora.Modules.Inventory.Mappers;

public static class CreateWarehouseMapper
{
    public static CreateWarehouseCommand ToCreateWarehouseCommand(CreateWarehouseRequest request)
    {
        return new CreateWarehouseCommand(Name: request.Name, IsDefault: request.IsDefault);
    }
}
