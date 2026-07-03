using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Domain;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Infrastructure;

public class AzureMediaObjectStorage : IMediaObjectStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly MediaStorageOptions _options;
    private readonly ILogger<AzureMediaObjectStorage> _logger;
    private readonly SemaphoreSlim _containerLock = new(1, 1);
    private readonly SemaphoreSlim _delegationKeyLock = new(1, 1);
    private BlobContainerClient? _containerClient;
    private UserDelegationKey? _delegationKey;
    private DateTimeOffset _delegationKeyExpiresAtUtc;

    public AzureMediaObjectStorage(
        BlobServiceClient blobServiceClient,
        IOptions<MediaStorageOptions> options,
        ILogger<AzureMediaObjectStorage> logger)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MediaUploadSasResult> CreateUploadSasAsync(
        string blobKey,
        string contentType,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);
        var sasBuilder = CreateSasBuilder(blobClient, expiresAtUtc);
        sasBuilder.SetPermissions(BlobSasPermissions.Create);

        var uri = await GenerateSasUriAsync(blobClient, sasBuilder, cancellationToken);
        return new MediaUploadSasResult(uri.ToString(), expiresAtUtc);
    }

    public async Task<MediaStoredObjectInfo?> GetObjectInfoAsync(string blobKey, CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);

        try
        {
            var response = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return new MediaStoredObjectInfo(
                response.Value.ContentLength,
                response.Value.ETag.ToString(),
                response.Value.ContentType ?? string.Empty);
        }
        catch (RequestFailedException exception) when (exception.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage could not read object properties.", exception);
        }
    }

    public async Task<MediaReadSasResult> CreateReadSasAsync(
        string blobKey,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);
        var sasBuilder = CreateSasBuilder(blobClient, expiresAtUtc);
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var uri = await GenerateSasUriAsync(blobClient, sasBuilder, cancellationToken);
        return new MediaReadSasResult(uri.ToString(), expiresAtUtc);
    }

    public async Task<Stream> OpenReadAsync(string blobKey, CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);

        try
        {
            return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage could not open the uploaded image.", exception);
        }
    }

    public async Task UploadAsync(string blobKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        try
        {
            await blobClient.UploadAsync(content, options, cancellationToken);
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage could not save the processed image.", exception);
        }
    }

    public async Task DeleteIfExistsAsync(string blobKey, CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobKey, cancellationToken);

        try
        {
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage could not delete an image.", exception);
        }
    }

    public async Task EnsureContainerAsync(CancellationToken cancellationToken)
    {
        if (_containerClient is not null)
            return;

        await _containerLock.WaitAsync(cancellationToken);
        try
        {
            if (_containerClient is not null)
                return;

            var container = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            _containerClient = container;
            _logger.LogInformation("Media storage container {ContainerName} is ready.", _options.ContainerName);
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage container is unavailable.", exception);
        }
        finally
        {
            _containerLock.Release();
        }
    }

    private async Task<BlobClient> GetBlobClientAsync(string blobKey, CancellationToken cancellationToken)
    {
        await EnsureContainerAsync(cancellationToken);
        return _containerClient!.GetBlobClient(blobKey);
    }

    private BlobSasBuilder CreateSasBuilder(BlobClient blobClient, DateTime expiresAtUtc)
    {
        return new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = new DateTimeOffset(expiresAtUtc),
            Protocol = IsLocalDevelopment() ? SasProtocol.HttpsAndHttp : SasProtocol.Https
        };
    }

    private async Task<Uri> GenerateSasUriAsync(
        BlobClient blobClient,
        BlobSasBuilder sasBuilder,
        CancellationToken cancellationToken)
    {
        if (blobClient.CanGenerateSasUri)
            return blobClient.GenerateSasUri(sasBuilder);

        var delegationKey = await GetDelegationKeyAsync(sasBuilder.ExpiresOn, cancellationToken);
        var parameters = sasBuilder.ToSasQueryParameters(delegationKey, _blobServiceClient.AccountName);
        return new UriBuilder(blobClient.Uri) { Query = parameters.ToString() }.Uri;
    }

    private async Task<UserDelegationKey> GetDelegationKeyAsync(DateTimeOffset sasExpiresAtUtc, CancellationToken cancellationToken)
    {
        if (_delegationKey is not null && _delegationKeyExpiresAtUtc > sasExpiresAtUtc.AddMinutes(5))
            return _delegationKey;

        await _delegationKeyLock.WaitAsync(cancellationToken);
        try
        {
            if (_delegationKey is not null && _delegationKeyExpiresAtUtc > sasExpiresAtUtc.AddMinutes(5))
                return _delegationKey;

            var startsAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5);
            var expiresAtUtc = DateTimeOffset.UtcNow.AddHours(2);
            var response = await _blobServiceClient.GetUserDelegationKeyAsync(startsAtUtc, expiresAtUtc, cancellationToken);
            _delegationKey = response.Value;
            _delegationKeyExpiresAtUtc = expiresAtUtc;
            return _delegationKey;
        }
        catch (RequestFailedException exception)
        {
            throw StorageUnavailable("Media storage could not authorize image access.", exception);
        }
        finally
        {
            _delegationKeyLock.Release();
        }
    }

    private bool IsLocalDevelopment() => _options.UsesConnectionString;

    private static StorageUnavailableException StorageUnavailable(string message, Exception exception) => new(message, exception);
}
