namespace backend.Modules.AiCoaching.Domain;

public enum WeeklyCoachGenerationFailureKind
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
