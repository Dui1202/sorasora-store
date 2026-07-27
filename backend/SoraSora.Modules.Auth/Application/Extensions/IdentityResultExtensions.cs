using Microsoft.AspNetCore.Identity;

namespace SoraSora.Modules.Auth.Application.Extensions;

public static class IdentityResultExtensions
{
    public static void ThrowIfFailed(this IdentityResult identityResult)
    {
        if (!identityResult.Succeeded)
        {
            throw new IdentityOperationException(identityResult.Errors);
        }
    }
}
