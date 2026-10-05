namespace SoraSora.Modules.Auth.Authorization;

public static class AppPermissions
{
    public static class Products
    {
        public const string Read = "Products.Read";
        public const string Create = "Products.Create";
        public const string Update = "Products.Update";
        public const string Delete = "Products.Delete";
    }

    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string UpdateRoles = "Users.UpdateRoles";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string UpdateRolePermissions = "Roles.UpdateRolePermissions";
        public const string Create = "Roles.Create";
        public const string Delete = "Roles.Delete";
        public const string Read = "Roles.Read";
    }

    public static class Orders
    {
        public const string Read = "Orders.Read";
        public const string PlaceOrder = "Orders.PlaceOrder";
        public const string Create = "Orders.Create";
        public const string Update = "Orders.Update";
        public const string Delete = "Orders.Delete";
    }

    public static class Warehouses
    {
        public const string Read = "Warehouses.Read";
        public const string Create = "Warehouses.Create";
        public const string Update = "Warehouses.Update";
        public const string Delete = "Warehouses.Delete";
    }

    public static class Inventory
    {
        public const string Read = "Inventory.Read";
        public const string Create = "Inventory.Create";
        public const string Update = "Inventory.Update";
        public const string Delete = "Inventory.Delete";
    }

    public static readonly IReadOnlyCollection<string> All =
    [
        Products.Read,
        Products.Create,
        Products.Update,
        Products.Delete,
        Users.Read,
        Users.Create,
        Users.UpdateRoles,
        Users.Delete,
        Roles.UpdateRolePermissions,
        Roles.Create,
        Roles.Delete,
        Roles.Read,
        Orders.PlaceOrder,
        Orders.Create,
        Orders.Read,
        Orders.Update,
        Orders.Delete,
        Warehouses.Create,
        Warehouses.Read,
        Warehouses.Update,
        Warehouses.Delete,
        Inventory.Create,
        Inventory.Read,
        Inventory.Update,
        Inventory.Delete,
    ];

    public static bool IsAppPermission(string permission)
    {
        return All.Contains(permission);
    }
}
