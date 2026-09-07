using SoraSora.Modules.Inventory.Domain;

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
