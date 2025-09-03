namespace backend.Modules.Shared.Service;

public interface IBlobService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileName);
}