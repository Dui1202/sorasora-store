using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Auth.Authorization;

namespace SoraSora.Modules.Auth.Infrastructure;

public sealed class IdentitySeeder(
    RoleManager<IdentityRole> roleManager,
    SoraSoraDbContext dbContext
)
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task SeedAsync()
    {
        await EnsureRoleAndPermission(AppRoles.Owner, AppPermissions.All);
    }

    private async Task EnsureRole(string roleName, IReadOnlyCollection<string> initialPermissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is not null)
        {
            return;
        }

        var newRole = new IdentityRole(roleName);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var creationResult = await _roleManager.CreateAsync(newRole);

        EnsureSucceeded(creationResult, $"Could not create '{roleName}'.");

        foreach (var permission in initialPermissions)
        {
            var claimResult = await _roleManager.AddClaimAsync(
                newRole,
                new Claim(AppClaimTypes.Permission, permission)
            );

            EnsureSucceeded(
                claimResult,
                $"Could not add permission '{permission}' " + $"to role '{roleName}'."
            );
        }

        await transaction.CommitAsync();
    }

    private async Task EnsureRoleAndPermission(
        string roleName,
        IReadOnlyCollection<string> permissions
    )
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role is null)
        {
            var newRole = new IdentityRole(roleName);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var result = await _roleManager.CreateAsync(newRole);

            result.ThrowIfFailed();

            foreach (var permission in permissions)
            {
                var claimResult = await _roleManager.AddClaimAsync(
                    newRole,
                    new Claim(AppClaimTypes.Permission, permission)
                );

                claimResult.ThrowIfFailed();
            }
            await transaction.CommitAsync();

            return;
        }

        var existingPermissions = (await _roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == AppClaimTypes.Permission)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        var permissionsToAdd = permissions
            .Except(existingPermissions, StringComparer.Ordinal)
            .ToArray();

        await using var secondTransaction = await _dbContext.Database.BeginTransactionAsync();

        foreach (var permission in permissionsToAdd)
        {
            var claimResult = await _roleManager.AddClaimAsync(
                role,
                new Claim(AppClaimTypes.Permission, permission)
            );

            claimResult.ThrowIfFailed();
        }

        await secondTransaction.CommitAsync();
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var descriptions = result.Errors.Select(error => error.Description);

        throw new InvalidOperationException($"{message} {string.Join(", ", descriptions)}");
    }
}
