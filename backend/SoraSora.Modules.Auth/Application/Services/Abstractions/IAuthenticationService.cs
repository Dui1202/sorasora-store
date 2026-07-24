using System.Security.Claims;
using SoraSora.Modules.Auth.Contracts.Responses;

namespace SoraSora.Modules.Auth.Application.Services.Abstractions
{
    public interface IAuthenticationService
    {
        Task<AuthenticatedUserResponse> SignInAsync(string email, string password, bool rememberMe);
        Task RegisterAsync(string email, string password);
        Task<AuthenticatedUserResponse?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task SignOutAsync();
    }
}
