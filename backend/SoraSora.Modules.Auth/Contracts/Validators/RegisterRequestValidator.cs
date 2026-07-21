using FluentValidation;

namespace SoraSora.Modules.Auth.Contracts.Validators
{
    public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
