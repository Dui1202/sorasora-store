using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Npgsql.Internal;
using SoraSora.Infrastructure.Identity;
using SoraSora.Modules.Auth.Contracts;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Auth.Application.Services
{
    public sealed class AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
    ) : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        public async Task<AuthenticatedUserResponse?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);

            if (user is null)
            {
                await _signInManager.SignOutAsync();

                return null;
            }

            return AuthenticatedUserResponse.Create(user);
        }

        public async Task RegisterAsync(string email, string password)
        {
            var trimmedEmail = email.Trim();
            var user = ApplicationUser.Create(trimmedEmail);

            var creationResult = await _userManager.CreateAsync(user, password);

            if (!creationResult.Succeeded)
            {
                throw new IdentityOperationException(creationResult.Errors);
            }

            return;
        }

        public async Task<AuthenticatedUserResponse> SignInAsync(
            string email,
            string password,
            bool rememberMe
        )
        {
            var trimmedEmail = email.Trim();
            var user =
                await _userManager.FindByEmailAsync(trimmedEmail)
                ?? throw new InvalidCredentialsException();

            var signInResult = await _signInManager.PasswordSignInAsync(
                user: user,
                password: password,
                isPersistent: rememberMe,
                lockoutOnFailure: true
            );

            if (!signInResult.Succeeded)
            {
                throw new InvalidCredentialsException();
            }

            return AuthenticatedUserResponse.Create(user);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();

            return;
        }
    }
}
