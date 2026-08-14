using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.AiCoaching.Domain;

public sealed class CoachMessage : Entity<Guid>
{
    public Guid ThreadId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? ReplyToMessageId { get; private set; }
    public int SequenceNumber { get; private set; }
    public CoachMessageRole Role { get; private set; }
    public CoachGenerationStatus Status { get; private set; }
    public Guid? ClientRequestId { get; private set; }
    public string? Question { get; private set; }
    public string? AnswerJson { get; private set; }
    public DateOnly? LocalRequestDate { get; private set; }
    public string? TimeZoneId { get; private set; }
    public Guid GenerationAttemptId { get; private set; }
    public string? SourceFingerprint { get; private set; }
    public string? SnapshotSchemaVersion { get; private set; }
    public string? ContextSnapshotJson { get; private set; }
    public string? PromptVersion { get; private set; }
    public string? ResponseSchemaVersion { get; private set; }
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
    public Guid ConcurrencyToken { get; private set; }

    public CoachThread Thread { get; private set; } = null!;
    public AppUser User { get; private set; } = null!;

    private CoachMessage()
    {
    }

    public static CoachMessage CreateUserQuestion(Guid id, Guid threadId, Guid userId, int sequenceNumber,
        Guid clientRequestId, string question, DateOnly localRequestDate, string timeZoneId, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureNonEmpty(id, "Message identity");
        AiCoachDomainRules.EnsureNonEmpty(threadId, "Thread identity");
        AiCoachDomainRules.EnsureNonEmpty(userId, "Message owner");
        AiCoachDomainRules.EnsureNonEmpty(clientRequestId, "Client request identity");
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        EnsureSequenceNumber(sequenceNumber);
        EnsureLocalDate(localRequestDate);

        return new CoachMessage
        {
            Id = id,
            ThreadId = threadId,
            UserId = userId,
            SequenceNumber = sequenceNumber,
            Role = CoachMessageRole.User,
            Status = CoachGenerationStatus.Completed,
            ClientRequestId = clientRequestId,
            Question = AiCoachDomainRules.NormalizeRequired(question, AiCoachInteractionLimits.MaximumQuestionLength, "Question"),
            LocalRequestDate = localRequestDate,
            TimeZoneId = AiCoachDomainRules.NormalizeRequired(timeZoneId, 100, "Timezone"),
            RequestedAt = utcNow,
            CompletedAt = utcNow,
            CreatedAt = utcNow,
            ConcurrencyToken = Guid.NewGuid()
        };
    }

    public static CoachMessage CreatePendingAssistant(Guid id, CoachMessage userQuestion, int sequenceNumber,
        DateTime utcNow)
    {
        AiCoachDomainRules.EnsureNonEmpty(id, "Message identity");
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (userQuestion.Role != CoachMessageRole.User || userQuestion.Id == Guid.Empty)
            throw new DomainException("An assistant response must reference a user question.");
        EnsureSequenceNumber(sequenceNumber);

        return new CoachMessage
        {
            Id = id,
            ThreadId = userQuestion.ThreadId,
            UserId = userQuestion.UserId,
            ReplyToMessageId = userQuestion.Id,
            SequenceNumber = sequenceNumber,
            Role = CoachMessageRole.Assistant,
            Status = CoachGenerationStatus.Pending,
            RequestedAt = utcNow,
            CreatedAt = utcNow,
            GenerationAttemptId = Guid.NewGuid(),
            ConcurrencyToken = Guid.NewGuid()
        };
    }

    public bool TryClaim(Guid attemptId, DateTime leaseExpiresAtUtc, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        AiCoachDomainRules.EnsureUtc(leaseExpiresAtUtc, nameof(leaseExpiresAtUtc));
        if (leaseExpiresAtUtc <= utcNow || Role != CoachMessageRole.Assistant || Status != CoachGenerationStatus.Pending ||
            GenerationAttemptId != attemptId)
        {
            return false;
        }

        Status = CoachGenerationStatus.Processing;
        ProcessingStartedAt = utcNow;
        ProcessingLeaseExpiresAt = leaseExpiresAtUtc;
        Touch(utcNow);
        return true;
    }

    public bool RequeueExpiredClaim(DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (Role != CoachMessageRole.Assistant || Status != CoachGenerationStatus.Processing ||
            ProcessingLeaseExpiresAt is null || ProcessingLeaseExpiresAt > utcNow)
        {
            return false;
        }

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
        ContextSnapshotJson = AiCoachDomainRules.NormalizeRequired(source.SnapshotJson, 100_000, "Context snapshot");
        PromptVersion = AiCoachDomainRules.NormalizeRequired(source.PromptVersion, 60, "Prompt version");
        ResponseSchemaVersion = AiCoachDomainRules.NormalizeRequired(source.ResponseSchemaVersion, 60, "Response schema version");
        Touch(utcNow);
    }

    public void Complete(Guid attemptId, string answerJson, CoachGenerationCompletion completion, DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        EnsureGenerationSource();
        AiCoachDomainRules.EnsureCompletionUsage(completion);

        AnswerJson = AiCoachDomainRules.NormalizeRequired(answerJson, 20_000, "Coach answer");
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
        if (Role != CoachMessageRole.Assistant || Status != CoachGenerationStatus.Failed)
            throw new DomainException("Only failed assistant messages can be retried.");

        Status = CoachGenerationStatus.Pending;
        GenerationAttemptId = Guid.NewGuid();
        SourceFingerprint = null;
        SnapshotSchemaVersion = null;
        ContextSnapshotJson = null;
        PromptVersion = null;
        ResponseSchemaVersion = null;
        RequestedAt = utcNow;
        FailedAt = null;
        ClearFailure();
        Touch(utcNow);
    }

    private void EnsureCurrentProcessingAttempt(Guid attemptId)
    {
        if (attemptId == Guid.Empty || Role != CoachMessageRole.Assistant || Status != CoachGenerationStatus.Processing ||
            GenerationAttemptId != attemptId)
        {
            throw new DomainException("The coach message generation attempt is no longer current.");
        }
    }

    private void EnsureGenerationSource()
    {
        if (string.IsNullOrWhiteSpace(ContextSnapshotJson) || string.IsNullOrWhiteSpace(SourceFingerprint))
            throw new DomainException("The coach message generation source is required.");
    }

    private static void EnsureSequenceNumber(int sequenceNumber)
    {
        if (sequenceNumber < 1)
            throw new DomainException("Message sequence number must be positive.");
    }

    private static void EnsureLocalDate(DateOnly localRequestDate)
    {
        if (localRequestDate == DateOnly.MinValue)
            throw new DomainException("Local request date is required.");
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
