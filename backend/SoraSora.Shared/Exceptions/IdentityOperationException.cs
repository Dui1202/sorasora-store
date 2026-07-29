using Microsoft.AspNetCore.Identity;

public sealed class IdentityOperationException(
    IEnumerable<IdentityError> errors,
    string message = "Identity operation failed."
) : Exception(message), SoraSora.Shared.Exceptions.IErrorCodeException
{
    public IReadOnlyCollection<IdentityError> Errors { get; } = errors.ToList();
    public string Code => "IDENTITY_OPERATION_FAILED";
}
