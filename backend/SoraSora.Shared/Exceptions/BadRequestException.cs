namespace SoraSora.Shared.Exceptions;

public class BadRequestException(
    string message,
    string code = "BAD_REQUEST",
    Exception? innerException = null
) : Exception(message, innerException), IErrorCodeException
{
    public string Code { get; } = code;
}
