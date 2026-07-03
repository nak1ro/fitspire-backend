using backend.Modules.Media.Domain;

namespace backend.Modules.Media.Contracts;

public record InitiateMediaUploadRequest(
    MediaPurpose Purpose,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? ClientRequestId = null);

public record MediaUploadSessionResponse(
    Guid MediaAssetId,
    MediaStatus Status,
    string? UploadUrl,
    string? Method,
    IReadOnlyDictionary<string, string>? RequiredHeaders,
    DateTime? ExpiresAtUtc);

public record MediaVariantResponse(
    MediaVariantKind Kind,
    int Width,
    int Height,
    string ContentType);

public record MediaAssetStatusResponse(
    Guid Id,
    MediaPurpose Purpose,
    MediaStatus Status,
    long? ActualSizeBytes,
    IReadOnlyList<MediaVariantResponse> Variants,
    DateTime CreatedAt,
    DateTime? ReadyAtUtc);

public record MediaReadVariantResponse(
    string Url,
    DateTime ExpiresAtUtc,
    int Width,
    int Height,
    string ContentType);

public record MediaResponse(
    Guid Id,
    MediaPurpose Purpose,
    MediaReadVariantResponse? Primary,
    MediaReadVariantResponse? Thumbnail);
