using SoraSora.Modules.Auth.Contracts;

namespace SoraSora.Modules.Auth.Application.Services
{
    public interface IAuthService
    {
        Task SignInAsync(SignInRequest request);
        Task RegisterAsync(RegisterRequest request);
    }
}
