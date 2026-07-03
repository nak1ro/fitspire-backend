namespace backend.Modules.Shared.Domain;

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
