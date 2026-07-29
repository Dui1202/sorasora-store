namespace SoraSora.Shared.Exceptions;

public class NotFoundException(
    string message,
    string code = "RESOURCE_NOT_FOUND",
    Exception? innerException = null
) : Exception(message, innerException), IErrorCodeException
{
    public string Code { get; } = code;
}
