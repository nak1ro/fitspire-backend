namespace backend.Modules.AiCoaching.Contracts;

public enum AiProviderFailureKind
{
    Configuration,
    Authentication,
    RateLimited,
    Timeout,
    Network,
    Refusal,
    IncompleteResponse,
    InvalidResponse,
    ProviderFailure
}

public sealed class AiServiceUnavailableException : Exception
{
    public AiServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class AiProviderException : Exception
{
    public AiProviderException(AiProviderFailureKind kind, string message, bool isRetryable,
        Exception? innerException = null, TimeSpan? retryAfter = null)
        : base(message, innerException)
    {
        Kind = kind;
        IsRetryable = isRetryable;
        RetryAfter = retryAfter;
    }

    public AiProviderFailureKind Kind { get; }
    public bool IsRetryable { get; }
    public TimeSpan? RetryAfter { get; }
}
