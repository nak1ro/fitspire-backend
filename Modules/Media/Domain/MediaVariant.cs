using backend.Modules.Shared.Domain;

namespace backend.Modules.Media.Domain;

public class MediaVariant : Entity<Guid>
{
    public Guid MediaAssetId { get; private set; }
    public MediaVariantKind Kind { get; private set; }
    public string BlobKey { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MediaAsset MediaAsset { get; private set; } = null!;

    private MediaVariant()
    {
    }

    internal static MediaVariant Create(
        Guid mediaAssetId,
        MediaVariantKind kind,
        string blobKey,
        long sizeBytes,
        int width,
        int height)
    {
        if (string.IsNullOrWhiteSpace(blobKey))
            throw new DomainException("Media variant storage key is required.");

        if (sizeBytes <= 0 || width <= 0 || height <= 0)
            throw new DomainException("Media variant dimensions and size must be positive.");

        return new MediaVariant
        {
            Id = Guid.NewGuid(),
            MediaAssetId = mediaAssetId,
            Kind = kind,
            BlobKey = blobKey,
            ContentType = MediaPolicies.NormalizedContentType,
            SizeBytes = sizeBytes,
            Width = width,
            Height = height,
            CreatedAt = DateTime.UtcNow
        };
    }
}
