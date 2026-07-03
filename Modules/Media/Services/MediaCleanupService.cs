using backend.Data;
using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Services;

public class MediaCleanupService
{
    private const long CleanupLockId = 4_219_043_611;

    private readonly FitspireDbContext _context;
    private readonly IMediaObjectStorage _storage;
    private readonly MediaStorageOptions _options;
    private readonly ILogger<MediaCleanupService> _logger;

    public MediaCleanupService(
        FitspireDbContext context,
        IMediaObjectStorage storage,
        IOptions<MediaStorageOptions> options,
        ILogger<MediaCleanupService> logger)
    {
        _context = context;
        _storage = storage;
        _options = options.Value;
        _logger = logger;
    }

    public async Task CleanExpiredMediaAsync(CancellationToken cancellationToken)
    {
        await _context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            if (!await TryAcquireLockAsync(cancellationToken))
                return;

            try
            {
                var assets = await FindCleanupCandidatesAsync(DateTime.UtcNow, cancellationToken);
                foreach (var asset in assets)
                    await CleanAssetAsync(asset, cancellationToken);
            }
            finally
            {
                await ReleaseLockAsync(cancellationToken);
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    private async Task<List<MediaAsset>> FindCleanupCandidatesAsync(DateTime now, CancellationToken cancellationToken)
    {
        var processingExpiresAt = now.AddMinutes(-_options.ProcessingTimeoutMinutes);
        var readyExpiresAt = now.AddMinutes(-_options.UnattachedReadyLifetimeMinutes);

        return await _context.MediaAssets
            .Include(asset => asset.Variants)
            .Where(asset =>
                (asset.Status == MediaStatus.Pending && asset.PendingExpiresAtUtc <= now) ||
                (asset.Status == MediaStatus.Processing && asset.ProcessingStartedAtUtc <= processingExpiresAt) ||
                (asset.Status == MediaStatus.Ready && asset.ReadyAtUtc <= readyExpiresAt) ||
                ((asset.Status == MediaStatus.Rejected || asset.Status == MediaStatus.Retired) &&
                 asset.NextCleanupAttemptAtUtc <= now))
            .OrderBy(asset => asset.NextCleanupAttemptAtUtc ?? asset.CreatedAt)
            .Take(_options.CleanupBatchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task CleanAssetAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        if (asset.Status is MediaStatus.Pending or MediaStatus.Processing or MediaStatus.Ready)
            asset.MarkRejected("Media was not attached before its retention period elapsed.", DateTime.UtcNow);

        try
        {
            await DeleteBlobKeysAsync(asset, cancellationToken);
            _context.MediaAssets.Remove(asset);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (StorageUnavailableException exception)
        {
            asset.ScheduleCleanupRetry(GetNextAttemptAt(asset.CleanupAttemptCount));
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(exception, "Media asset {MediaAssetId} cleanup will be retried.", asset.Id);
        }
    }

    private async Task DeleteBlobKeysAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        await _storage.DeleteIfExistsAsync(asset.StagingBlobKey, cancellationToken);
        foreach (var variant in asset.Variants)
            await _storage.DeleteIfExistsAsync(variant.BlobKey, cancellationToken);
    }

    private async Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken)
    {
        return (bool)(await ExecuteLockCommandAsync("SELECT pg_try_advisory_lock(@lockId)", cancellationToken) ?? false);
    }

    private async Task ReleaseLockAsync(CancellationToken cancellationToken)
    {
        await ExecuteLockCommandAsync("SELECT pg_advisory_unlock(@lockId)", cancellationToken);
    }

    private async Task<object?> ExecuteLockCommandAsync(string commandText, CancellationToken cancellationToken)
    {
        await using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = commandText;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "lockId";
        parameter.Value = CleanupLockId;
        command.Parameters.Add(parameter);
        return await command.ExecuteScalarAsync(cancellationToken);
    }

    private static DateTime GetNextAttemptAt(int attemptCount)
    {
        var delayMinutes = Math.Min(60, 5 * (int)Math.Pow(2, Math.Min(attemptCount, 4)));
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }
}
