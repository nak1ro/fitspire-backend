using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace backend.Modules.Shared.Service;

public class BlobService : IBlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(IConfiguration config)
    {
        var connStr = config["Azure:BlobConnectionString"];
        var containerName = config["Azure:ProfilePicsContainer"];
        
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("Azure:BlobConnectionString is missing or empty in configuration.");
        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("Azure:ProfilePicsContainer is missing or empty in configuration.");
        
        _containerClient = new BlobContainerClient(connStr, containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(fileStream, options, cancellationToken: default);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }
}