using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace backend.Modules.Shared.Service;

public class BlobService : IBlobService
{
    private const string LocalUploadPath = "uploads/profile-pictures";

    private readonly BlobContainerClient? _containerClient;
    private readonly IWebHostEnvironment _environment;
    private readonly bool _useAzureStorage;

    public BlobService(IConfiguration config, IWebHostEnvironment environment)
    {
        var connStr = config["Azure:BlobConnectionString"];
        var containerName = config["Azure:ProfilePicsContainer"];
        _environment = environment;
        _useAzureStorage = !string.IsNullOrWhiteSpace(connStr) && !string.IsNullOrWhiteSpace(containerName);

        if (_useAzureStorage)
        {
            _containerClient = new BlobContainerClient(connStr, containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!_useAzureStorage)
            return await UploadLocalFileAsync(fileStream, fileName);

        var blobClient = _containerClient!.GetBlobClient(fileName);
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
        if (!_useAzureStorage)
        {
            DeleteLocalFile(fileName);
            return;
        }

        var blobClient = _containerClient!.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }

    private async Task<string> UploadLocalFileAsync(Stream fileStream, string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var uploadDirectory = GetLocalUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(uploadDirectory, safeFileName);
        await using var outputStream = File.Create(filePath);
        await fileStream.CopyToAsync(outputStream);

        return $"/{LocalUploadPath}/{safeFileName}";
    }

    private void DeleteLocalFile(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(GetLocalUploadDirectory(), safeFileName);

        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    private string GetLocalUploadDirectory()
    {
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

        return Path.Combine(webRootPath, LocalUploadPath);
    }
}
