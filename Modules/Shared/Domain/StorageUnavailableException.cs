namespace backend.Modules.Shared.Domain;

public class StorageUnavailableException : Exception
{
    public StorageUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
