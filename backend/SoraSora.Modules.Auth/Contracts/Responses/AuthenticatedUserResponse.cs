using SoraSora.Infrastructure.Identity;

namespace SoraSora.Modules.Auth.Contracts.Responses;

public sealed record AuthenticatedUserResponse(string Id, string Email)
{
    public static AuthenticatedUserResponse Create(ApplicationUser user)
    {
        return new AuthenticatedUserResponse(
            Id: user.Id,
            Email: user.Email
                ?? throw new InvalidOperationException(
                    "An authenticated user must have an email address."
                )
        );
    }
}
