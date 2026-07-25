using System.ComponentModel.DataAnnotations.Schema;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.AiCoaching.Domain;

public sealed class WeeklyCoachReport : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public string TimeZoneId { get; private set; } = null!;
    public WeeklyCoachReportStatus Status { get; private set; }
    public Guid GenerationAttemptId { get; private set; }
    public int GenerationCount { get; private set; }
    public string SourceFingerprint { get; private set; } = null!;
    public string SnapshotSchemaVersion { get; private set; } = null!;
    public string SnapshotJson { get; private set; } = null!;
    public string PromptVersion { get; private set; } = null!;
    public string ResponseSchemaVersion { get; private set; } = null!;
    public string? ReportJson { get; private set; }
    public string? Provider { get; private set; }
    public string? ProviderResponseId { get; private set; }
    public string? Model { get; private set; }
    public int? InputTokens { get; private set; }
    public int? OutputTokens { get; private set; }
    public int? TotalTokens { get; private set; }
    public WeeklyCoachGenerationFailureKind? LastFailureKind { get; private set; }
    public string? LastFailureMessage { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? ProcessingStartedAt { get; private set; }
    public DateTime? ProcessingLeaseExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public Guid ConcurrencyToken { get; private set; }

    public AppUser User { get; private set; } = null!;

    [NotMapped]
    public bool HasDisplayableContent => !string.IsNullOrWhiteSpace(ReportJson);

    private WeeklyCoachReport()
    {
    }

    public static WeeklyCoachReport CreatePending(
        Guid id,
        Guid userId,
        DateOnly periodStart,
        string timeZoneId,
        WeeklyCoachReportSource source,
        DateTime utcNow)
    {
        EnsureIdentity(id, userId);
        EnsurePeriod(periodStart);
        EnsureUtc(utcNow, nameof(utcNow));

        var report = new WeeklyCoachReport
        {
            Id = id,
            UserId = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodStart.AddDays(6),
            TimeZoneId = NormalizeRequired(timeZoneId, 100, "Timezone"),
            Status = WeeklyCoachReportStatus.Pending,
            GenerationCount = 1,
            RequestedAt = utcNow,
            CreatedAt = utcNow
        };
        report.ApplySource(source);
        report.StartNewAttempt();
        return report;
    }

    public bool MatchesCompletedSource(string sourceFingerprint) =>
        Status == WeeklyCoachReportStatus.Completed &&
        string.Equals(SourceFingerprint, sourceFingerprint, StringComparison.Ordinal);

    public void QueueReplacement(WeeklyCoachReportSource source, DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        if (Status is WeeklyCoachReportStatus.Pending or WeeklyCoachReportStatus.Processing)
            throw new DomainException("A coaching report generation is already in progress.");

        ApplySource(source);
        Status = WeeklyCoachReportStatus.Pending;
        GenerationCount++;
        RequestedAt = utcNow;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        FailedAt = null;
        ClearFailure();
        StartNewAttempt();
        Touch(utcNow);
    }

    public bool TryClaim(Guid attemptId, DateTime leaseExpiresAtUtc, DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        EnsureUtc(leaseExpiresAtUtc, nameof(leaseExpiresAtUtc));
        if (leaseExpiresAtUtc <= utcNow || Status != WeeklyCoachReportStatus.Pending || GenerationAttemptId != attemptId)
            return false;

        Status = WeeklyCoachReportStatus.Processing;
        ProcessingStartedAt = utcNow;
        ProcessingLeaseExpiresAt = leaseExpiresAtUtc;
        Touch(utcNow);
        return true;
    }

    public bool RequeueExpiredClaim(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        if (Status != WeeklyCoachReportStatus.Processing || ProcessingLeaseExpiresAt is null || ProcessingLeaseExpiresAt > utcNow)
            return false;

        Status = WeeklyCoachReportStatus.Pending;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        Touch(utcNow);
        return true;
    }

    public void Complete(
        Guid attemptId,
        string reportJson,
        WeeklyCoachCompletion completion,
        DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        EnsureUtc(utcNow, nameof(utcNow));
        ValidateCompletion(reportJson, completion);

        ReportJson = reportJson.Trim();
        Provider = NormalizeRequired(completion.Provider, 40, "Provider");
        ProviderResponseId = NormalizeRequired(completion.ProviderResponseId, 200, "Provider response ID");
        Model = NormalizeRequired(completion.Model, 100, "Model");
        InputTokens = completion.InputTokens;
        OutputTokens = completion.OutputTokens;
        TotalTokens = completion.TotalTokens;
        Status = WeeklyCoachReportStatus.Completed;
        CompletedAt = utcNow;
        FailedAt = null;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        ClearFailure();
        Touch(utcNow);
    }

    public void Fail(Guid attemptId, WeeklyCoachGenerationFailureKind failureKind, string safeMessage, DateTime utcNow)
    {
        EnsureCurrentProcessingAttempt(attemptId);
        EnsureUtc(utcNow, nameof(utcNow));

        Status = WeeklyCoachReportStatus.Failed;
        LastFailureKind = failureKind;
        LastFailureMessage = NormalizeRequired(safeMessage, 300, "Failure message");
        FailedAt = utcNow;
        ProcessingStartedAt = null;
        ProcessingLeaseExpiresAt = null;
        Touch(utcNow);
    }

    private void ApplySource(WeeklyCoachReportSource source)
    {
        SourceFingerprint = NormalizeRequired(source.SourceFingerprint, 128, "Source fingerprint");
        SnapshotSchemaVersion = NormalizeRequired(source.SnapshotSchemaVersion, 60, "Snapshot schema version");
        SnapshotJson = NormalizeRequired(source.SnapshotJson, 100_000, "Snapshot");
        PromptVersion = NormalizeRequired(source.PromptVersion, 60, "Prompt version");
        ResponseSchemaVersion = NormalizeRequired(source.ResponseSchemaVersion, 60, "Response schema version");
    }

    private void StartNewAttempt()
    {
        GenerationAttemptId = Guid.NewGuid();
        ConcurrencyToken = Guid.NewGuid();
    }

    private void EnsureCurrentProcessingAttempt(Guid attemptId)
    {
        if (attemptId == Guid.Empty || Status != WeeklyCoachReportStatus.Processing || GenerationAttemptId != attemptId)
            throw new DomainException("The coaching report generation attempt is no longer current.");
    }

    private static void EnsureIdentity(Guid id, Guid userId)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("Coaching report identity and owner are required.");
    }

    private static void EnsurePeriod(DateOnly periodStart)
    {
        if (periodStart == DateOnly.MinValue || periodStart.DayOfWeek != DayOfWeek.Monday)
            throw new DomainException("A coaching report period must begin on Monday.");
    }

    private static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException($"{name} must be in UTC.");
    }

    private static void ValidateCompletion(string reportJson, WeeklyCoachCompletion completion)
    {
        if (string.IsNullOrWhiteSpace(reportJson) || reportJson.Length > 100_000)
            throw new DomainException("Coaching report content is required and must be within the allowed size.");
        if (completion.InputTokens < 0 || completion.OutputTokens < 0 || completion.TotalTokens < 0)
            throw new DomainException("AI token usage cannot be negative.");
    }

    private static string NormalizeRequired(string value, int maximumLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
            throw new DomainException($"{name} is required and must be at most {maximumLength} characters.");
        return normalized;
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

public sealed record WeeklyCoachReportSource(
    string SourceFingerprint,
    string SnapshotSchemaVersion,
    string SnapshotJson,
    string PromptVersion,
    string ResponseSchemaVersion);

public sealed record WeeklyCoachCompletion(
    string Provider,
    string ProviderResponseId,
    string Model,
    int InputTokens,
    int OutputTokens,
    int TotalTokens);
