namespace SoraSora.Shared.Exceptions;

public class ConflictException(
    string message,
    string code = "CONFLICT",
    Exception? innerException = null
) : Exception(message, innerException), IErrorCodeException
{
    public string Code { get; } = code;
}
