using Microsoft.AspNetCore.Authorization;

namespace SoraSora.Modules.Auth.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
