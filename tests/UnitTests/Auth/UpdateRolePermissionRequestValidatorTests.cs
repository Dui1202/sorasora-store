using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Requests;
using SoraSora.Modules.Auth.Contracts.Validators;

namespace UnitTests.Auth;

public sealed class UpdateRolePermissionRequestValidatorTests
{
    private readonly UpdateRolePermissionRequestValidator _validator = new();

    [Fact]
    public async Task RejectsNullPermissions()
    {
        var request = new UpdateRolePermissionRequest(null!);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Permissions));
    }

    [Fact]
    public async Task RejectsUnknownPermission()
    {
        var request = new UpdateRolePermissionRequest(["Unknown.Permission"]);

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == "Permissions[0]");
    }

    [Fact]
    public async Task AcceptsDuplicatePermissions()
    {
        var request = new UpdateRolePermissionRequest(
            [AppPermissions.Users.Read, AppPermissions.Users.Read]
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsEmptyPermissions()
    {
        var request = new UpdateRolePermissionRequest([]);

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsValidPermissions()
    {
        var request = new UpdateRolePermissionRequest(
            [AppPermissions.Users.Read, AppPermissions.Users.UpdateRoles]
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
