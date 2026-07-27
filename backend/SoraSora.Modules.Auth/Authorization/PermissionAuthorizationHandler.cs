using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;

namespace SoraSora.Modules.Auth.Authorization;

public sealed class PermissionAuthorizationHandler(SoraSoraDbContext dbContext)
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly SoraSoraDbContext _dbContext = dbContext;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var hasPermission = await (
            from userRoles in _dbContext.UserRoles
            join roleClaims in _dbContext.RoleClaims on userRoles.RoleId equals roleClaims.RoleId
            where
                userId == userRoles.UserId
                && roleClaims.ClaimType == AppClaimTypes.Permission
                && roleClaims.ClaimValue == requirement.Permission
            select roleClaims.RoleId
        ).AnyAsync();

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
