using SoraSora.Modules.Inventory.Contracts.Responds;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Mappers;

public static class WarehouseDtoMapper
{
    public static WarehouseResponse ToWarehouseResponse(Warehouse warehouse)
    {
        return new WarehouseResponse(
            Id: warehouse.Id,
            Name: warehouse.Name,
            IsDefault: warehouse.IsDefault,
            IsArchive: warehouse.IsArchive
        );
    }
}
