using Microsoft.AspNetCore.Identity;

public sealed class IdentityOperationException(
    IEnumerable<IdentityError> errors,
    string message = "Identity operation failed."
) : Exception(message)
{
    public IReadOnlyCollection<IdentityError> Errors { get; } = errors.ToList();
}
