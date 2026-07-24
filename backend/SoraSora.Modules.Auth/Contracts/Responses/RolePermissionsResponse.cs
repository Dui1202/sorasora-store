namespace SoraSora.Modules.Auth.Contracts.Responses
{
    public sealed record RolePermissionsResponse(
        string RoleName,
        IReadOnlyCollection<string> Permissions
    );
}
