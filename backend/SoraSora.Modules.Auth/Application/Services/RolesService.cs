using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Auth.Application.Services.Abstractions;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Auth.Application.Services
{
    public sealed class RolesService(
        RoleManager<IdentityRole> roleManager,
        SoraSoraDbContext dbContext
    ) : IRolesService
    {
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<RolePermissionsResponse> CreateRoleWithPermissionsAsync(
            string roleName,
            IReadOnlyCollection<string> permissions
        )
        {
            var trimmedRoleName = roleName.Trim();
            var existingRole = await _roleManager.FindByNameAsync(trimmedRoleName);
            if (existingRole is not null)
            {
                throw new BadRequestException($"Role '{existingRole.Name}' has already existed");
            }

            var normalizedPermissions = permissions.Distinct(StringComparer.Ordinal).ToList();

            if (!ArePermissionsValid(normalizedPermissions, out var invalidPermissions))
            {
                throw new BadRequestException(
                    $"Invalid application permissions: {string.Join(", ", invalidPermissions)}."
                );
            }
            var newRole = new IdentityRole(trimmedRoleName);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var creationResult = await _roleManager.CreateAsync(newRole);

            creationResult.ThrowIfFailed();

            foreach (var permission in normalizedPermissions)
            {
                var result = await _roleManager.AddClaimAsync(
                    newRole,
                    new Claim(AppClaimTypes.Permission, permission)
                );

                result.ThrowIfFailed();
            }

            await transaction.CommitAsync();

            return new RolePermissionsResponse(newRole.Name!, normalizedPermissions);
        }

        public async Task<IReadOnlyList<RolePermissionsResponse>> GetAllRolesWithPermissionsAsync()
        {
            var roles = await _roleManager.Roles.OrderBy(role => role.Name).ToListAsync();
            var response = new List<RolePermissionsResponse>();
            foreach (var role in roles)
            {
                var permissions = (await _roleManager.GetClaimsAsync(role))
                    .Where(claim => claim.Type == AppClaimTypes.Permission)
                    .Select(claim => claim.Value)
                    .ToArray();

                response.Add(new RolePermissionsResponse(role.Name!, permissions));
            }

            return response;
        }

        public async Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
            string roleName,
            IReadOnlyCollection<string> requestedPermissions
        )
        {
            var trimmedRoleName = roleName.Trim();
            var role =
                await _roleManager.FindByNameAsync(trimmedRoleName)
                ?? throw new NotFoundException($"Role '{trimmedRoleName}' does not exist.");

            var normalizedPermissions = requestedPermissions
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (!ArePermissionsValid(normalizedPermissions, out var invalidPermissions))
            {
                throw new BadRequestException(
                    $"Invalid application permissions: {string.Join(", ", invalidPermissions)}."
                );
            }

            var existingPermissions = (await _roleManager.GetClaimsAsync(role))
                .Where(claim => claim.Type == AppClaimTypes.Permission)
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.Ordinal);

            var permissionsToAdd = normalizedPermissions
                .Except(existingPermissions, StringComparer.Ordinal)
                .ToArray();

            var permissionsToRemove = existingPermissions
                .Except(normalizedPermissions, StringComparer.Ordinal)
                .ToArray();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            foreach (var permission in permissionsToAdd)
            {
                var claimResult = await _roleManager.AddClaimAsync(
                    role,
                    new Claim(AppClaimTypes.Permission, permission)
                );

                claimResult.ThrowIfFailed();

                existingPermissions.Add(permission);
            }

            foreach (var permission in permissionsToRemove)
            {
                var claimResult = await _roleManager.RemoveClaimAsync(
                    role,
                    new Claim(AppClaimTypes.Permission, permission)
                );

                claimResult.ThrowIfFailed();

                existingPermissions.Remove(permission);
            }

            await transaction.CommitAsync();

            return new RolePermissionsResponse(role.Name!, existingPermissions);
        }

        private static bool ArePermissionsValid(
            IReadOnlyCollection<string> permissions,
            out IReadOnlyCollection<string> invalidPermissions
        )
        {
            invalidPermissions = permissions
                .Where(permission => !AppPermissions.IsAppPermission(permission))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (invalidPermissions.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}
