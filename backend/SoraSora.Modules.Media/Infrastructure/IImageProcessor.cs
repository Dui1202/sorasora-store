using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Media.Infrastructure;

public interface IImageProcessor
{
    ProcessedImage Process(Stream source, long sourceLength, CancellationToken cancellationToken);
}
