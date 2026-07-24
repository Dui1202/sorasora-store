using SoraSora.Modules.Auth.Contracts.Responses;

namespace SoraSora.Modules.Auth.Application.Services.Abstractions
{
    public interface IRolesService
    {
        Task<RolePermissionsResponse> UpdateRolePermissionsAsync(
            string roleName,
            IReadOnlyCollection<string> permissions
        );
        Task<IReadOnlyList<RolePermissionsResponse>> GetAllRolesWithPermissionsAsync();
        Task<RolePermissionsResponse> CreateRoleWithPermissionsAsync(
            string roleName,
            IReadOnlyCollection<string> permission
        );
    }
}
