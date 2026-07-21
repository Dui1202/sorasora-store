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
    ];
}
