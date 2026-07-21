using SoraSora.Modules.Auth.Contracts;
using SoraSora.Modules.Auth.Contracts.Validators;

namespace UnitTests.Auth;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task RejectsMissingEmail()
    {
        var request = new RegisterRequest("", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsMalformedEmail()
    {
        var request = new RegisterRequest("not-an-email", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsEmailLongerThan256Characters()
    {
        var request = new RegisterRequest($"{new string('a', 245)}@example.com", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Email));
    }

    [Fact]
    public async Task RejectsPasswordShorterThan8Characters()
    {
        var request = new RegisterRequest("user@example.com", "Pass1!");

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Password));
    }

    [Fact]
    public async Task AcceptsValidRegistrationCredentials()
    {
        var request = new RegisterRequest("user@example.com", "Password1!");

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
