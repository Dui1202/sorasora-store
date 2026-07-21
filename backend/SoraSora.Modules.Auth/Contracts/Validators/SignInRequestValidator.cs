using FluentValidation;

namespace SoraSora.Modules.Auth.Contracts.Validators
{
    public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
    {
        public SignInRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();

            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
