using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Application.Services;
using SoraSora.Modules.Auth.Contracts;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Auth.Application.Controllers
{
    [ApiController]
    [Route(ApiRoute.AuthApiRoute)]
    public sealed class AuthenticationController(IAuthenticationService authenticationService)
        : ControllerBase
    {
        private readonly IAuthenticationService _authenticateService = authenticationService;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticatedUserResponse>> RegisterAsync(
            [FromForm] RegisterRequest request,
            [FromServices] IValidator<RegisterRequest> validator,
            CancellationToken cancellationToken
        )
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return this.ValidationProblem(validationResult);
            }

            await _authenticateService.RegisterAsync(request.Email, request.Password);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<ActionResult<AuthenticatedUserResponse>> SignInAsync(
            [FromForm] SignInRequest request,
            [FromServices] IValidator<SignInRequest> validator,
            CancellationToken cancellationToken
        )
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return this.ValidationProblem(validationResult);
            }

            var user = await _authenticateService.SignInAsync(
                request.Email,
                request.Password,
                request.RememberMe
            );

            return Ok(user);
        }

        [Authorize]
        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOutAsync()
        {
            await _authenticateService.SignOutAsync();

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUserAsync()
        {
            var user = await _authenticateService.GetCurrentUserAsync(User);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }
    }
}
