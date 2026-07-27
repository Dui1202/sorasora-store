using SoraSora.Modules.Auth.Contracts.Responses;

namespace SoraSora.Modules.Auth.Application.Services.Abstractions;

public interface IUserRolesService
{
    Task<UserRoleResponse> UpdateUserRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken
    );
    Task<UserRoleResponse> GetUserRoleByUserIdAsync(
        string userId,
        CancellationToken cancellationToken
    );
}
