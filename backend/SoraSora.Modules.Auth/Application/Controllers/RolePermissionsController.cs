using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Application.Services.Abstractions;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Requests;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Auth.Application.Controllers
{
    [ApiController]
    [Route(ApiRoute.RolesApiRoute)]
    public sealed class RolePermissionsController(IRolesService rolesService) : ControllerBase
    {
        private readonly IRolesService _roleService = rolesService;

        [HasPermission(AppPermissions.Roles.UpdateRolePermissions)]
        [HttpPut("{roleName}/permissions")]
        public async Task<ActionResult<RolePermissionsResponse>> UpdateRolePermissionsAsync(
            string roleName,
            UpdateRolePermissionRequest request,
            [FromServices] IValidator<UpdateRolePermissionRequest> validator,
            CancellationToken cancellationToken
        )
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return this.ValidationProblem(validationResult);
            }

            var role = await _roleService.UpdateRolePermissionsAsync(roleName, request.Permissions);

            return Ok(role);
        }

        [HasPermission(AppPermissions.Roles.Read)]
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RolePermissionsResponse>>> GetAll()
        {
            var roles = await _roleService.GetAllRolesWithPermissionsAsync();

            return Ok(roles);
        }

        [HasPermission(AppPermissions.Roles.Create)]
        [HttpPost]
        public async Task<ActionResult<RolePermissionsResponse>> Create(
            CreateRoleRequest request,
            [FromServices] IValidator<CreateRoleRequest> validator,
            CancellationToken cancellationToken
        )
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return this.ValidationProblem(validationResult);
            }

            var role = await _roleService.CreateRoleWithPermissionsAsync(
                roleName: request.RoleName,
                permission: request.Permissions
            );

            return Ok(role);
        }
    }
}
