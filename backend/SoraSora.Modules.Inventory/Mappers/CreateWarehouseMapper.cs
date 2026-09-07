public static class CreateWarehouseMapper
{
    public static CreateWarehouseCommand ToCreateWarehouseCommand(CreateWarehouseRequest request)
    {
        return new CreateWarehouseCommand(Name: request.Name, IsDefault: request.IsDefault);
    }
}
