using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Media.Exceptions
{
    public class MediaProcessingException(string message, Exception? innerException = null)
        : BadRequestException(message, MediaErrorCodes.MediaProcessingFailed, innerException) { }
}
