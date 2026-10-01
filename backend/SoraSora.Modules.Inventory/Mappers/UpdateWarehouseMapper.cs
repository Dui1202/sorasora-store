using SoraSora.Modules.Inventory.Contracts.Commands;
using SoraSora.Modules.Inventory.Contracts.Requests;

namespace SoraSora.Modules.Inventory.Mappers;

public static class UpdateWarehouseMapper
{
    public static UpdateWarehouseCommand ToUpdateWarehouseCommand(UpdateWarehouseRequest request)
    {
        return new UpdateWarehouseCommand(
            Id: request.Id,
            Name: request.Name,
            IsDefault: request.IsDefault
        );
    }
}
