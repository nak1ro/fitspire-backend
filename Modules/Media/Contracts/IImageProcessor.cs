using backend.Modules.Media.Domain;

namespace backend.Modules.Media.Contracts;

public interface IImageProcessor
{
    Task<IReadOnlyList<ProcessedMediaVariant>> ProcessAsync(
        Stream source,
        MediaPurpose purpose,
        CancellationToken cancellationToken);
}

public record ProcessedMediaVariant(
    MediaVariantKind Kind,
    byte[] Content,
    int Width,
    int Height,
    string ContentType);
