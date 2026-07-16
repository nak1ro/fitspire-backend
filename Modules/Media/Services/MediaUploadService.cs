using AutoMapper;
using backend.Data;
using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using System.Data;

namespace backend.Modules.Media.Services;

public class MediaUploadService : IMediaUploadService
{
    private readonly FitspireDbContext _context;
    private readonly IMediaObjectStorage _storage;
    private readonly IImageProcessor _imageProcessor;
    private readonly MediaStorageOptions _options;
    private readonly IMapper _mapper;
    private readonly ILogger<MediaUploadService> _logger;

    public MediaUploadService(
        FitspireDbContext context,
        IMediaObjectStorage storage,
        IImageProcessor imageProcessor,
        IOptions<MediaStorageOptions> options,
        IMapper mapper,
        ILogger<MediaUploadService> logger)
    {
        _context = context;
        _storage = storage;
        _imageProcessor = imageProcessor;
        _options = options.Value;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MediaUploadSessionResponse> InitiateAsync(
        InitiateMediaUploadRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var asset = await FindOrCreatePendingAssetAsync(request, userId, now, cancellationToken);

        if (asset.Status is MediaStatus.Ready or MediaStatus.Attached)
            return new MediaUploadSessionResponse(asset.Id, asset.Status, null, null, null, null);

        if (!asset.IsPendingAndUsable(now))
            throw new DomainException("Media upload is no longer available. Start a new upload.");

        var expiresAtUtc = now.AddMinutes(_options.UploadSasMinutes);
        asset.RefreshUploadUrlExpiration(expiresAtUtc, now);
        await _context.SaveChangesAsync(cancellationToken);

        var sas = await _storage.CreateUploadSasAsync(
            asset.StagingBlobKey,
            asset.DeclaredContentType,
            expiresAtUtc,
            cancellationToken);

        return new MediaUploadSessionResponse(
            asset.Id,
            asset.Status,
            sas.Url,
            "PUT",
            new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob",
                ["Content-Type"] = asset.DeclaredContentType
            },
            sas.ExpiresAtUtc);
    }

    public async Task<MediaAssetStatusResponse> CompleteAsync(
        Guid mediaAssetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var asset = await ClaimForProcessingAsync(mediaAssetId, userId, cancellationToken);
        if (asset.Status is MediaStatus.Ready or MediaStatus.Attached)
            return MapStatus(asset);

        var uploadedKeys = new List<string>();
        try
        {
            var objectInfo = await GetVerifiedObjectInfoAsync(asset, cancellationToken);
            var variants = await ProcessAndStoreVariantsAsync(asset, uploadedKeys, cancellationToken);
            await _storage.DeleteIfExistsAsync(asset.StagingBlobKey, cancellationToken);

            asset.MarkReady(objectInfo.ETag, objectInfo.SizeBytes, variants, DateTime.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);
            return MapStatus(asset);
        }
        catch (StorageUnavailableException)
        {
            throw;
        }
        catch (Exception exception) when (exception is DomainException or InvalidImageContentException)
        {
            await RejectAndCleanAsync(asset, uploadedKeys, exception.Message, cancellationToken);
            throw new DomainException(exception.Message, exception);
        }
    }

    public async Task<MediaAssetStatusResponse> GetStatusAsync(
        Guid mediaAssetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var asset = await FindOwnedAssetAsync(mediaAssetId, userId, cancellationToken);
        return MapStatus(asset);
    }

    public async Task AbortAsync(Guid mediaAssetId, Guid userId, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var asset = await FindOwnedAssetAsync(mediaAssetId, userId, cancellationToken);

        if (asset.Status == MediaStatus.Attached)
            throw new DomainException("Attached media must be removed through its profile or post.");

        asset.Retire(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<MediaAsset> FindOrCreatePendingAssetAsync(
        InitiateMediaUploadRequest request,
        Guid userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var existing = await FindExistingInitiationAsync(request.ClientRequestId, userId, cancellationToken);
        if (existing is not null)
        {
            EnsureMatchingInitiation(existing, request);
            await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        var pendingCount = await _context.MediaAssets.CountAsync(
            asset => asset.OwnerUserId == userId && asset.Status == MediaStatus.Pending && asset.PendingExpiresAtUtc > now,
            cancellationToken);
        if (pendingCount >= _options.MaxPendingUploadsPerUser)
            throw new DomainException("Too many pending media uploads. Complete or cancel an existing upload first.");

        var uploadExpiry = now.AddMinutes(_options.UploadSasMinutes);
        var asset = MediaAsset.CreatePending(
            userId,
            request.Purpose,
            request.FileName,
            request.ContentType,
            request.SizeBytes,
            CreateStagingBlobKey(userId),
            request.ClientRequestId,
            uploadExpiry,
            now.AddMinutes(_options.PendingUploadLifetimeMinutes));

        _context.MediaAssets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return asset;
    }

    private async Task<MediaAsset> ClaimForProcessingAsync(
        Guid mediaAssetId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var asset = await FindOwnedAssetAsync(mediaAssetId, userId, cancellationToken);
        if (asset.Status is MediaStatus.Ready or MediaStatus.Attached)
        {
            await transaction.CommitAsync(cancellationToken);
            return asset;
        }

        asset.BeginProcessing(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return asset;
    }

    private async Task<MediaStoredObjectInfo> GetVerifiedObjectInfoAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        var objectInfo = await _storage.GetObjectInfoAsync(asset.StagingBlobKey, cancellationToken)
            ?? throw new DomainException("Uploaded image could not be found in storage.");

        if (objectInfo.SizeBytes != asset.DeclaredSizeBytes)
            throw new DomainException("Uploaded image size does not match the initiated upload.");

        if (objectInfo.SizeBytes > GetMaximumSize(asset.Purpose))
            throw new DomainException("Uploaded image exceeds the allowed size.");

        if (!string.Equals(objectInfo.ContentType, asset.DeclaredContentType, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Uploaded image content type does not match the initiated upload.");

        return objectInfo;
    }

    private async Task<IReadOnlyList<MediaVariant>> ProcessAndStoreVariantsAsync(
        MediaAsset asset,
        List<string> uploadedKeys,
        CancellationToken cancellationToken)
    {
        await using var source = await _storage.OpenReadAsync(asset.StagingBlobKey, cancellationToken);
        var processedVariants = await _imageProcessor.ProcessAsync(source, asset.Purpose, cancellationToken);
        var variants = new List<MediaVariant>(processedVariants.Count);

        foreach (var processedVariant in processedVariants)
        {
            var blobKey = CreateVariantBlobKey(asset, processedVariant.Kind);
            await using var content = new MemoryStream(processedVariant.Content, writable: false);
            await _storage.UploadAsync(blobKey, content, processedVariant.ContentType, cancellationToken);
            uploadedKeys.Add(blobKey);
            variants.Add(MediaVariant.Create(
                asset.Id,
                processedVariant.Kind,
                blobKey,
                processedVariant.Content.LongLength,
                processedVariant.Width,
                processedVariant.Height));
        }

        return variants;
    }

    private async Task RejectAndCleanAsync(
        MediaAsset asset,
        IEnumerable<string> uploadedKeys,
        string reason,
        CancellationToken cancellationToken)
    {
        await DeletePartialVariantsAsync(uploadedKeys, cancellationToken);
        asset.MarkRejected(reason, DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeletePartialVariantsAsync(IEnumerable<string> uploadedKeys, CancellationToken cancellationToken)
    {
        foreach (var key in uploadedKeys)
        {
            try
            {
                await _storage.DeleteIfExistsAsync(key, cancellationToken);
            }
            catch (StorageUnavailableException exception)
            {
                _logger.LogWarning(exception, "Media variant cleanup will be retried later.");
            }
        }
    }

    private async Task<MediaAsset?> FindExistingInitiationAsync(string? clientRequestId, Guid userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientRequestId))
            return null;

        return await _context.MediaAssets
            .Include(asset => asset.Variants)
            .FirstOrDefaultAsync(asset => asset.OwnerUserId == userId && asset.ClientRequestId == clientRequestId.Trim(), cancellationToken);
    }

    private async Task<MediaAsset> FindOwnedAssetAsync(Guid mediaAssetId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.MediaAssets
            .Include(asset => asset.Variants)
            .FirstOrDefaultAsync(asset => asset.Id == mediaAssetId && asset.OwnerUserId == userId, cancellationToken)
            ?? throw new NotFoundException("Media upload was not found.");
    }

    private void EnsureMatchingInitiation(MediaAsset asset, InitiateMediaUploadRequest request)
    {
        if (!asset.MatchesInitiation(request.Purpose, request.ContentType, request.SizeBytes, request.FileName))
            throw new DomainException("Client request ID was already used for different media metadata.");
    }

    private long GetMaximumSize(MediaPurpose purpose) => purpose switch
    {
        MediaPurpose.ProfilePicture => _options.ProfilePictureMaxBytes,
        MediaPurpose.PostImage => _options.PostImageMaxBytes,
        MediaPurpose.BodyProgressPhoto => _options.BodyProgressPhotoMaxBytes,
        _ => throw new DomainException("Unsupported media purpose.")
    };

    private static string CreateStagingBlobKey(Guid userId) => $"staging/{userId:N}/{Guid.NewGuid():N}";

    private static string CreateVariantBlobKey(MediaAsset asset, MediaVariantKind kind) =>
        $"media/{asset.OwnerUserId:N}/{asset.Id:N}/{kind.ToString().ToLowerInvariant()}.webp";

    private MediaAssetStatusResponse MapStatus(MediaAsset asset) => _mapper.Map<MediaAssetStatusResponse>(asset);
}
