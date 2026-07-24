namespace SoraSora.Modules.Auth.Contracts.Requests
{
    public sealed record CreateRoleRequest(
        string RoleName,
        IReadOnlyCollection<string> Permissions
    );
}
