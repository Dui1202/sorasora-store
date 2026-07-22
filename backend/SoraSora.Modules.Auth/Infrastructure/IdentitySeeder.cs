using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SoraSora.Modules.Auth.Authorization;

namespace SoraSora.Modules.Auth.Infrastructure
{
    public sealed class IdentitySeeder(RoleManager<IdentityRole> roleManager)
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task SeedAsync()
        {
            await EnsureRole(AppRoles.Owner, AppPermissions.All);
        }

        private async Task EnsureRole(
            string roleName,
            IReadOnlyCollection<string> initialPermissions
        )
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                return;
            }

            var newRole = new IdentityRole(roleName);

            var creationResult = await _roleManager.CreateAsync(newRole);

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
}
