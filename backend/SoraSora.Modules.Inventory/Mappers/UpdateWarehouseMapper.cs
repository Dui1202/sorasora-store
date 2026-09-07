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
