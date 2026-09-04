public interface IWarehouseService
{
    Task CreateWarehouseAsync();
    Task GetAllWarehousesAsync();
    Task GetWarehouseByIdAsync(string id);
    Task UpdateWarehouseAsync();
    Task DeleteWarehouseByIdAsync(string id);
}
