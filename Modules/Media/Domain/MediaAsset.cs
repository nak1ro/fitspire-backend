using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Media.Domain;

public class MediaAsset : Entity<Guid>
{
    public Guid OwnerUserId { get; private set; }
    public MediaPurpose Purpose { get; private set; }
    public MediaStatus Status { get; private set; }
    public string? ClientRequestId { get; private set; }
    public string OriginalFileName { get; private set; } = null!;
    public string DeclaredContentType { get; private set; } = null!;
    public long DeclaredSizeBytes { get; private set; }
    public string StagingBlobKey { get; private set; } = null!;
    public string? UploadedETag { get; private set; }
    public long? ActualSizeBytes { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime UploadUrlExpiresAtUtc { get; private set; }
    public DateTime PendingExpiresAtUtc { get; private set; }
    public DateTime? ProcessingStartedAtUtc { get; private set; }
    public DateTime? ReadyAtUtc { get; private set; }
    public DateTime? AttachedAtUtc { get; private set; }
    public DateTime? RetiredAtUtc { get; private set; }
    public int CleanupAttemptCount { get; private set; }
    public DateTime? NextCleanupAttemptAtUtc { get; private set; }
    public DateTime? ModerationRemovedAtUtc { get; private set; }
    public bool IsModerationRemoved => ModerationRemovedAtUtc is not null;

    public AppUser OwnerUser { get; private set; } = null!;
    public ICollection<MediaVariant> Variants { get; private set; } = new List<MediaVariant>();

    private MediaAsset()
    {
    }

    public static MediaAsset CreatePending(
        Guid ownerUserId,
        MediaPurpose purpose,
        string originalFileName,
        string declaredContentType,
        long declaredSizeBytes,
        string stagingBlobKey,
        string? clientRequestId,
        DateTime uploadUrlExpiresAtUtc,
        DateTime pendingExpiresAtUtc)
    {
        if (ownerUserId == Guid.Empty)
            throw new DomainException("Media owner is required.");

        if (!MediaPolicies.SupportedContentTypes.Contains(declaredContentType))
            throw new DomainException("Unsupported image content type.");

        if (declaredSizeBytes <= 0)
            throw new DomainException("Media size must be positive.");

        if (string.IsNullOrWhiteSpace(stagingBlobKey))
            throw new DomainException("Media staging key is required.");

        return new MediaAsset
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            Purpose = purpose,
            Status = MediaStatus.Pending,
            ClientRequestId = NormalizeOptional(clientRequestId),
            OriginalFileName = NormalizeFileName(originalFileName),
            DeclaredContentType = declaredContentType.Trim().ToLowerInvariant(),
            DeclaredSizeBytes = declaredSizeBytes,
            StagingBlobKey = stagingBlobKey,
            UploadUrlExpiresAtUtc = uploadUrlExpiresAtUtc,
            PendingExpiresAtUtc = pendingExpiresAtUtc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool MatchesInitiation(MediaPurpose purpose, string contentType, long sizeBytes, string fileName)
    {
        return Purpose == purpose
               && DeclaredSizeBytes == sizeBytes
               && string.Equals(DeclaredContentType, contentType.Trim(), StringComparison.OrdinalIgnoreCase)
               && string.Equals(OriginalFileName, NormalizeFileName(fileName), StringComparison.Ordinal);
    }

    public bool IsPendingAndUsable(DateTime utcNow) => Status == MediaStatus.Pending && PendingExpiresAtUtc > utcNow;

    public void RefreshUploadUrlExpiration(DateTime expiresAtUtc, DateTime utcNow)
    {
        if (Status != MediaStatus.Pending)
            throw new DomainException("Only pending media can receive a new upload URL.");

        UploadUrlExpiresAtUtc = expiresAtUtc;
        UpdatedAt = utcNow;
    }

    public void BeginProcessing(DateTime utcNow)
    {
        if (Status is MediaStatus.Ready or MediaStatus.Attached)
            return;

        if (Status != MediaStatus.Pending)
            throw new DomainException("Media upload cannot be completed in its current state.");

        if (PendingExpiresAtUtc <= utcNow)
            throw new DomainException("Media upload has expired.");

        Status = MediaStatus.Processing;
        ProcessingStartedAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void MarkReady(string uploadedETag, long actualSizeBytes, IReadOnlyCollection<MediaVariant> variants, DateTime utcNow)
    {
        if (Status != MediaStatus.Processing)
            throw new DomainException("Only processing media can become ready.");

        if (actualSizeBytes <= 0 || variants.Count == 0)
            throw new DomainException("Completed media must include verified content and variants.");

        if (variants.Select(x => x.Kind).Distinct().Count() != variants.Count)
            throw new DomainException("Media variants must not repeat a kind.");

        UploadedETag = uploadedETag;
        ActualSizeBytes = actualSizeBytes;
        foreach (var variant in variants)
            Variants.Add(variant);

        Status = MediaStatus.Ready;
        ReadyAtUtc = utcNow;
        ProcessingStartedAtUtc = null;
        FailureReason = null;
        UpdatedAt = utcNow;
    }

    public void MarkRejected(string reason, DateTime utcNow)
    {
        if (Status is MediaStatus.Attached or MediaStatus.Retired)
            throw new DomainException("Attached or retired media cannot be rejected.");

        Status = MediaStatus.Rejected;
        FailureReason = NormalizeReason(reason);
        ProcessingStartedAtUtc = null;
        NextCleanupAttemptAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void Attach(DateTime utcNow)
    {
        if (Status == MediaStatus.Attached)
            return;

        if (Status != MediaStatus.Ready)
            throw new DomainException("Only ready media can be attached.");

        Status = MediaStatus.Attached;
        AttachedAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void Retire(DateTime utcNow)
    {
        if (Status == MediaStatus.Retired)
            return;

        Status = MediaStatus.Retired;
        RetiredAtUtc = utcNow;
        NextCleanupAttemptAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void RemoveByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation removal time must be in UTC.");
        if (ModerationRemovedAtUtc is not null)
            return;

        ModerationRemovedAtUtc = utcNow;
        UpdatedAt = utcNow;
    }

    public void RestoreByModeration(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("Moderation restoration time must be in UTC.");
        if (ModerationRemovedAtUtc is null)
            return;

        ModerationRemovedAtUtc = null;
        UpdatedAt = utcNow;
    }

    public void ScheduleCleanupRetry(DateTime nextAttemptAtUtc)
    {
        CleanupAttemptCount++;
        NextCleanupAttemptAtUtc = nextAttemptAtUtc;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeFileName(string fileName)
    {
        var value = Path.GetFileName(fileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Media file name is required.");

        return value.Length <= 255 ? value : value[..255];
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeReason(string reason)
    {
        var value = string.IsNullOrWhiteSpace(reason) ? "Media processing failed." : reason.Trim();
        return value.Length <= 500 ? value : value[..500];
    }
}
