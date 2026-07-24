using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Requests;
using SoraSora.Modules.Auth.Contracts.Validators;

namespace UnitTests.Auth;

public sealed class CreateRoleRequestValidatorTests
{
    private readonly CreateRoleRequestValidator _validator = new();

    [Fact]
    public async Task RejectsMissingRoleName()
    {
        var request = new CreateRoleRequest("", []);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.RoleName));
    }

    [Fact]
    public async Task RejectsRoleNameLongerThan256Characters()
    {
        var request = new CreateRoleRequest(new string('a', 257), []);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.RoleName));
    }

    [Fact]
    public async Task AcceptsRoleNameWithSurroundingWhitespace()
    {
        var request = new CreateRoleRequest(" Admin ", []);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RejectsNullPermissions()
    {
        var request = new CreateRoleRequest("Admin", null!);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Permissions));
    }

    [Fact]
    public async Task RejectsUnknownPermission()
    {
        var request = new CreateRoleRequest("Admin", ["Unknown.Permission"]);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == "Permissions[0]");
    }

    [Fact]
    public async Task AcceptsDuplicatePermissions()
    {
        var request = new CreateRoleRequest(
            "Admin",
            [AppPermissions.Products.Read, AppPermissions.Products.Read]
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsRoleWithoutPermissions()
    {
        var request = new CreateRoleRequest("Viewer", []);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsValidRoleAndPermissions()
    {
        var request = new CreateRoleRequest(
            "CatalogManager",
            [AppPermissions.Products.Read, AppPermissions.Products.Update]
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
