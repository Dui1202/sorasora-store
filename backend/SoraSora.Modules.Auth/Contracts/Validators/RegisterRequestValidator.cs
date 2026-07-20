using FluentValidation;
using Microsoft.AspNetCore.Identity.Data;
using SoraSora.Modules.Auth.Contracts;

namespace SoraSora.Modules.Auth.Contracts.Validators
{
    public sealed class RegisterRequestValidator : AbstractValidator<LoginRequest> { }
}
