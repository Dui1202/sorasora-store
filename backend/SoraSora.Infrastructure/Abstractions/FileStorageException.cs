namespace SoraSora.Infrastructure.Abstractions;

public sealed class FileStorageException(
    string operation,
    string objectPath,
    Exception innerException
) : Exception($"File storage {operation} failed for object '{objectPath}'.", innerException);
