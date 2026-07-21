using SoraSora.Modules.Auth.Contracts;
using SoraSora.Modules.Auth.Contracts.Validators;

namespace UnitTests.Auth;

public sealed class SignInRequestValidatorTests
{
    private readonly SignInRequestValidator _validator = new();

    [Fact]
    public async Task RejectsMissingEmail()
    {
        var request = new SignInRequest("", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsMalformedEmail()
    {
        var request = new SignInRequest("not-an-email", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsEmailLongerThan256Characters()
    {
        var request = new SignInRequest($"{new string('a', 245)}@example.com", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsMissingPassword()
    {
        var request = new SignInRequest("user@example.com", "");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Password));
    }

    [Fact]
    public async Task AcceptsValidSignInCredentials()
    {
        var request = new SignInRequest("user@example.com", "Password1!", RememberMe: true);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
