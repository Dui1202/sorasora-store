using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Auth.Application.Services.Abstractions;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Auth.Application.Services
{
    public sealed class UserRolesService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SoraSoraDbContext dbContext
    ) : IUserRolesService
    {
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<UserRoleResponse> GetUserRoleByUserIdAsync(
            string userId,
            CancellationToken cancellationToken
        )
        {
            var response = await (
                from user in _dbContext.Users
                join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                join role in _dbContext.Roles on userRole.RoleId equals role.Id
                where user.Id == userId
                select new UserRoleResponse(user.Id, role.Name!)
            ).SingleOrDefaultAsync(cancellationToken);

            return response
                ?? throw new NotFoundException(
                    $"User with ID '{userId}' was not found or has no role."
                );
        }

        public async Task<UserRoleResponse> UpdateUserRoleAsync(
            string userId,
            string roleName,
            CancellationToken cancellationToken
        )
        {
            var trimmedRoleName = roleName.Trim();
            var user =
                await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException($"The user with ID: '{userId}' is not found!");

            var role =
                await _roleManager.FindByNameAsync(trimmedRoleName)
                ?? throw new BadRequestException($"Role '{trimmedRoleName}' does not exist");

            var userCurrentRole = await _userManager.GetRolesAsync(user);

            if (
                userCurrentRole.Count == 1
                && string.Equals(userCurrentRole[0], role.Name, StringComparison.OrdinalIgnoreCase)
            )
            {
                return new UserRoleResponse(user.Id, role.Name!);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            if (userCurrentRole.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, userCurrentRole);

                removeResult.ThrowIfFailed();
            }

            var addResult = await _userManager.AddToRoleAsync(user, role.Name!);

            addResult.ThrowIfFailed();

            await transaction.CommitAsync(cancellationToken);

            return new UserRoleResponse(user.Id, role.Name!);
        }
    }
}
