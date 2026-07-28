namespace SoraSora.Shared.Exceptions;

public class ConflictException(
    string message,
    string code = "conflict",
    Exception? innerException = null
) : Exception(message, innerException)
{
    public string Code { get; } = code;
}
