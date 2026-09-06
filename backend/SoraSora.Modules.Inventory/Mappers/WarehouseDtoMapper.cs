using SoraSora.Modules.Inventory.Domain;

public static class WarehouseDtoMapper
{
    public static WarehouseResponse ToWarehouseResponse(Warehouse warehouse)
    {
        return new WarehouseResponse(Name: warehouse.Name, IsDefault: warehouse.IsDefault);
    }
}
