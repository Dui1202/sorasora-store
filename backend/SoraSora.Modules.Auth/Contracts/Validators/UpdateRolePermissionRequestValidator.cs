using FluentValidation;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Requests;

namespace SoraSora.Modules.Auth.Contracts.Validators;

public sealed class UpdateRolePermissionRequestValidator
    : AbstractValidator<UpdateRolePermissionRequest>
{
    public UpdateRolePermissionRequestValidator()
    {
        RuleFor(request => request.Permissions)
            .Cascade(CascadeMode.Stop)
            .NotNull();

        RuleForEach(request => request.Permissions)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(AppPermissions.IsAppPermission)
            .WithMessage("'{PropertyValue}' is not a valid application permission.");
    }
}
