namespace SoraSora.Modules.Auth.Contracts.Requests;

public sealed record UpdateRolePermissionRequest(IReadOnlyCollection<string> Permissions);
