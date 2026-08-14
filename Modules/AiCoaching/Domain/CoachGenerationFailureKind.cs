namespace backend.Modules.AiCoaching.Domain;

public enum CoachGenerationFailureKind
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
