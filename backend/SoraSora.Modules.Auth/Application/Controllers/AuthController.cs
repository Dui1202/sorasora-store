using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoraSora.Modules.Auth.Application.Services;

namespace SoraSora.Modules.Auth.Application.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        public async Task<IActionResult> Register()
        {
            throw new NotImplementedException();
        }
    }
}
