using FluentValidation;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Requests;

namespace SoraSora.Modules.Auth.Contracts.Validators;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    private const int MaximumRoleNameLength = 256;

    public CreateRoleRequestValidator()
    {
        RuleFor(request => request.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(MaximumRoleNameLength);

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
