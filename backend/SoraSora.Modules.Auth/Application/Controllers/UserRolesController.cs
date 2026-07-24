using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Application.Services.Abstractions;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Auth.Application.Controllers
{
    [ApiController]
    [Route(ApiRoute.UsersApiRoute)]
    public sealed class UserRolesController(IUserRolesService userRolesService) : ControllerBase
    {
        private readonly IUserRolesService _userRolesService = userRolesService;

        [HasPermission(AppPermissions.Users.UpdateRoles)]
        [HttpPut("{userId}/role")]
        public async Task<ActionResult<UserRoleResponse>> UpdateUserRoleAsync(
            string userId,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            var userRole = await _userRolesService.UpdateUserRoleAsync(
                userId,
                roleName,
                cancellationToken
            );

            return Ok(userRole);
        }

        [HasPermission(AppPermissions.Users.Read)]
        [HttpGet("{userId}/role")]
        public async Task<ActionResult<UserRoleResponse>> GetUserRoleByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default
        )
        {
            var userRole = await _userRolesService.GetUserRoleByUserIdAsync(
                userId,
                cancellationToken
            );

            return Ok(userRole);
        }
    }
}
