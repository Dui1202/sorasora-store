using Microsoft.AspNetCore.Identity;
using SoraSora.Modules.Auth.Application.Extensions;

namespace UnitTests.Auth;

public sealed class IdentityResultExtensionsTests
{
    [Fact]
    public void DoesNotThrowForSuccessfulResult()
    {
        IdentityResult.Success.ThrowIfFailed();
    }

    [Fact]
    public void ThrowsIdentityOperationExceptionWithOriginalErrorsForFailedResult()
    {
        var errors = new[]
        {
            new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = "The role already exists.",
            },
            new IdentityError
            {
                Code = "InvalidRoleName",
                Description = "The role name is invalid.",
            },
        };
        var result = IdentityResult.Failed(errors);

        var exception = Assert.Throws<IdentityOperationException>(() => result.ThrowIfFailed());

        Assert.Equal(errors, exception.Errors);
    }
}
