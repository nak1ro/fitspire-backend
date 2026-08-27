using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.AiCoaching.Domain;

public sealed class DailyCoachBriefing : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public DateOnly LocalDate { get; private set; }
    public string TimeZoneId { get; private set; } = null!;
    public CoachGenerationStatus Status { get; private set; }
    public Guid GenerationAttemptId { get; private set; }
    public string? SourceFingerprint { get; private set; }
    public string? SnapshotSchemaVersion { get; private set; }
    public string? SnapshotJson { get; private set; }
    public string? PromptVersion { get; private set; }
    public string? ResponseSchemaVersion { get; private set; }
    public string? ContentJson { get; private set; }
    public string? Provider { get; private set; }
    public string? ProviderResponseId { get; private set; }
    public string? Model { get; private set; }
    public int? InputTokens { get; private set; }
    public int? OutputTokens { get; private set; }
    public int? TotalTokens { get; private set; }
    public CoachGenerationFailureKind? LastFailureKind { get; private set; }
    public string? LastFailureMessage { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? ProcessingStartedAt { get; private set; }
    public DateTime? ProcessingLeaseExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public int RefreshCount { get; private set; }
    public Guid ConcurrencyToken { get; private set; }

    public AppUser User { get; private set; } = null!;

    private DailyCoachBriefing()
    {
    }

    public static DailyCoachBriefing CreatePending(Guid id, Guid userId, DateOnly localDate, string timeZoneId,
        DateTime utcNow)
    {
        AiCoachDomainRules.EnsureNonEmpty(id, "Daily briefing identity");
        AiCoachDomainRules.EnsureNonEmpty(userId, "Daily briefing owner");
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (localDate == DateOnly.MinValue)
            throw new DomainException("Daily briefing local date is required.");

        return new DailyCoachBriefing
        {
            Id = id,
            UserId = userId,
            LocalDate = localDate,
            TimeZoneId = AiCoachDomainRules.NormalizeRequired(timeZoneId, 100, "Timezone"),
            Status = CoachGenerationStatus.Pending,
            GenerationAttemptId = Guid.NewGuid(),
            RequestedAt = utcNow,
            CreatedAt = utcNow,
            ConcurrencyToken = Guid.NewGuid()
        };
    }

    public bool TryClaim(Guid attemptId, DateTime leaseExpiresAtUtc, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        AiCoachDomainRules.EnsureUtc(leaseExpiresAtUtc, nameof(leaseExpiresAtUtc));
        if (leaseExpiresAtUtc <= utcNow || Status != CoachGenerationStatus.Pending || GenerationAttemptId != attemptId)
            return false;

        Status = CoachGenerationStatus.Processing;
        ProcessingStartedAt = utcNow;
        ProcessingLeaseExpiresAt = leaseExpiresAtUtc;
        Touch(utcNow);
        return true;
    }

    public bool RequeueExpiredClaim(DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (Status != CoachGenerationStatus.Processing || ProcessingLeaseExpiresAt is null || ProcessingLeaseExpiresAt > utcNow)
            return false;

        Status = CoachGenerationStatus.Pending;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        Touch(utcNow);
        return true;
    }

    public void SetGenerationSource(Guid attemptId, CoachGenerationSource source, DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));

        SourceFingerprint = AiCoachDomainRules.NormalizeRequired(source.SourceFingerprint, 128, "Source fingerprint");
        SnapshotSchemaVersion = AiCoachDomainRules.NormalizeRequired(source.SnapshotSchemaVersion, 60, "Snapshot schema version");
        SnapshotJson = AiCoachDomainRules.NormalizeRequired(source.SnapshotJson, 100_000, "Daily briefing snapshot");
        PromptVersion = AiCoachDomainRules.NormalizeRequired(source.PromptVersion, 60, "Prompt version");
        ResponseSchemaVersion = AiCoachDomainRules.NormalizeRequired(source.ResponseSchemaVersion, 60, "Response schema version");
        Touch(utcNow);
    }

    public void Complete(Guid attemptId, string contentJson, CoachGenerationCompletion completion, DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        EnsureGenerationSource();
        AiCoachDomainRules.EnsureCompletionUsage(completion);

        ContentJson = AiCoachDomainRules.NormalizeRequired(contentJson, 20_000, "Daily briefing content");
        Provider = AiCoachDomainRules.NormalizeRequired(completion.Provider, 40, "Provider");
        ProviderResponseId = AiCoachDomainRules.NormalizeRequired(completion.ProviderResponseId, 200, "Provider response ID");
        Model = AiCoachDomainRules.NormalizeRequired(completion.Model, 100, "Model");
        InputTokens = completion.InputTokens;
        OutputTokens = completion.OutputTokens;
        TotalTokens = completion.TotalTokens;
        Status = CoachGenerationStatus.Completed;
        CompletedAt = utcNow;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        ClearFailure();
        Touch(utcNow);
    }

    public void Fail(Guid attemptId, CoachGenerationFailureKind failureKind, string safeMessage, DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));

        Status = CoachGenerationStatus.Failed;
        LastFailureKind = failureKind;
        LastFailureMessage = AiCoachDomainRules.NormalizeRequired(safeMessage, 300, "Failure message");
        FailedAt = utcNow;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        Touch(utcNow);
    }

    public void Retry(DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (Status != CoachGenerationStatus.Failed)
            throw new DomainException("Only failed daily briefings can be retried.");

        Status = CoachGenerationStatus.Pending;
        GenerationAttemptId = Guid.NewGuid();
        SourceFingerprint = null;
        SnapshotSchemaVersion = null;
        SnapshotJson = null;
        PromptVersion = null;
        ResponseSchemaVersion = null;
        RequestedAt = utcNow;
        FailedAt = null;
        ClearFailure();
        Touch(utcNow);
    }

    public bool TryRefreshAfterActivity(DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (Status != CoachGenerationStatus.Completed || RefreshCount >= 1)
            return false;

        Status = CoachGenerationStatus.Pending;
        GenerationAttemptId = Guid.NewGuid();
        RequestedAt = utcNow;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        CompletedAt = null;
        ContentJson = null;
        ClearFailure();
        RefreshCount++;
        Touch(utcNow);
        return true;
    }

    public void Regenerate(DateTime utcNow)
    {
        if (!TryRefreshAfterActivity(utcNow))
            throw new DomainException("Today's daily briefing cannot be regenerated again.");
    }

    private void EnsureCurrentProcessingAttempt(Guid attemptId)
    {
        if (attemptId == Guid.Empty || Status != CoachGenerationStatus.Processing || GenerationAttemptId != attemptId)
            throw new DomainException("The daily briefing generation attempt is no longer current.");
    }

    private void EnsureGenerationSource()
    {
        if (string.IsNullOrWhiteSpace(SnapshotJson) || string.IsNullOrWhiteSpace(SourceFingerprint))
            throw new DomainException("The daily briefing generation source is required.");
    }

    private void ClearFailure()
    {
        LastFailureKind = null;
        LastFailureMessage = null;
    }

    private void Touch(DateTime utcNow)
    {
        UpdatedAt = utcNow;
        ConcurrencyToken = Guid.NewGuid();
    }
}
