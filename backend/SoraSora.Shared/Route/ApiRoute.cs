namespace SoraSora.Shared.Route;

public static class ApiRoute
{
    public const string ApiRoot = "/api";
    public const string ProductsApiRoute = ApiRoot + "/products";
    public const string AuthApiRoute = ApiRoot + "/auth";
    public const string RolesApiRoute = AuthApiRoute + "/roles";
    public const string UsersApiRoute = AuthApiRoute + "/users";
    public const string InventoryApiRoute = ApiRoot + "/inventory";
    public const string WarehouseApiRoute = ApiRoot + "/warehouses";
    public const string OrderApiRoute = ApiRoot + "/orders";
}
