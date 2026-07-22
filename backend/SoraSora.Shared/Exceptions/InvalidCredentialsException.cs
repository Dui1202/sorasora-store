namespace SoraSora.Shared.Exceptions
{
    public sealed class InvalidCredentialsException(
        string message = "The email or password is incorrect"
    ) : Exception(message) { }
}
