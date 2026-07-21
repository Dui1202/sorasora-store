using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Infrastructure.Identity;
using SoraSora.Modules.Auth.Contracts;
using SoraSora.Modules.Auth.Contracts.Responses;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Auth.Application.Controllers
{
    [ApiController]
    [Route(ApiRoute.AuthApiRoute)]
    public sealed class AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
    ) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

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

            var email = request.Email.Trim();
            var user = ApplicationUser.Create(email);

            var creationResult = await _userManager.CreateAsync(user, request.Password);

            if (!creationResult.Succeeded)
            {
                return ValidationProblem(
                    new ValidationProblemDetails(
                        creationResult
                            .Errors.GroupBy(error => error.Code)
                            .ToDictionary(
                                group => group.Key,
                                group => group.Select(error => error.Description).ToArray()
                            )
                    )
                );
            }

            var reponse = AuthenticatedUserResponse.Create(user);

            return StatusCode(StatusCodes.Status201Created, reponse);
        }

        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<ActionResult<AuthenticatedUserResponse>> SignInAsync(
            [FromForm] SignInRequest request,
            [FromServices] IValidator<SignInRequest> validator
        )
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return this.ValidationProblem(validationResult);
            }

            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return InvalidCredentials();
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                user,
                password: request.Password,
                isPersistent: request.RememberMe,
                lockoutOnFailure: true
            );

            if (!signInResult.Succeeded)
            {
                return InvalidCredentials();
            }

            return Ok(AuthenticatedUserResponse.Create(user));
        }

        [Authorize]
        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOutAsync()
        {
            await _signInManager.SignOutAsync();

            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                await _signInManager.SignOutAsync();

                return Unauthorized();
            }

            return Ok(AuthenticatedUserResponse.Create(user));
        }

        private ObjectResult InvalidCredentials()
        {
            return Problem(
                title: "Invalid credentials",
                detail: "The email or password is incorrect",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
    }
}
