namespace SoraSora.Shared.Exceptions;

public class ExternalServiceException(
    string message,
    string code = "EXTERNAL_SERVICE_UNAVAILABLE",
    Exception? innerException = null
) : Exception(message, innerException), IErrorCodeException
{
    public string Code { get; } = code;
}
