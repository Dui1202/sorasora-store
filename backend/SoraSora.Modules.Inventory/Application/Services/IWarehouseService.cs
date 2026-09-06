public interface IWarehouseService
{
    Task<WarehouseResponse> CreateWarehouseAsync(
        CreateWarehouseCommand command,
        CancellationToken cancellationToken
    );
    Task<IReadOnlyCollection<WarehouseResponse>?> GetAllWarehousesAsync(
        CancellationToken cancellationToken
    );
    Task<WarehouseResponse> GetWarehouseByIdAsync(string id, CancellationToken cancellationToken);
    Task<WarehouseResponse> UpdateWarehouseAsync(
        UpdateWarehouseCommand command,
        CancellationToken cancellationToken
    );
    Task DeleteWarehouseByIdAsync(string id, CancellationToken cancellationToken);
}
