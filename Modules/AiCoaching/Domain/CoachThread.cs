using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.AiCoaching.Domain;

public sealed class CoachThread : AggregateRoot<Guid>
{
    private const string DefaultTitle = "New conversation";

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public bool HasCustomTitle { get; private set; }
    public string? ContextSummary { get; private set; }
    public int NextSequenceNumber { get; private set; }
    public int LastSummarySequenceNumber { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public Guid ConcurrencyToken { get; private set; }

    public AppUser User { get; private set; } = null!;
    public ICollection<CoachMessage> Messages { get; private set; } = new List<CoachMessage>();

    private CoachThread()
    {
    }

    public static CoachThread Create(Guid id, Guid userId, string? title, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureNonEmpty(id, "Thread identity");
        AiCoachDomainRules.EnsureNonEmpty(userId, "Thread owner");
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));

        var normalizedTitle = string.IsNullOrWhiteSpace(title)
            ? DefaultTitle
            : AiCoachDomainRules.NormalizeRequired(title, AiCoachInteractionLimits.MaximumThreadTitleLength, "Thread title");

        return new CoachThread
        {
            Id = id,
            UserId = userId,
            Title = normalizedTitle,
            HasCustomTitle = !string.IsNullOrWhiteSpace(title),
            LastActivityAt = utcNow,
            CreatedAt = utcNow,
            ConcurrencyToken = Guid.NewGuid()
        };
    }

    public void Rename(string title, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        Title = AiCoachDomainRules.NormalizeRequired(title, AiCoachInteractionLimits.MaximumThreadTitleLength, "Thread title");
        HasCustomTitle = true;
        Touch(utcNow);
    }

    public int ReserveNextSequenceNumber(DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (NextSequenceNumber == int.MaxValue)
            throw new DomainException("The conversation has reached its message limit.");

        NextSequenceNumber++;
        Touch(utcNow);
        return NextSequenceNumber;
    }

    public void ApplyAutomaticTitle(string question, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (HasCustomTitle || NextSequenceNumber > 2)
            return;

        var normalizedQuestion = AiCoachDomainRules.NormalizeRequired(question,
            AiCoachInteractionLimits.MaximumQuestionLength, "Question");
        var generatedTitle = normalizedQuestion.Length > AiCoachInteractionLimits.MaximumThreadTitleLength
            ? normalizedQuestion[..AiCoachInteractionLimits.MaximumThreadTitleLength].TrimEnd()
            : normalizedQuestion;

        Title = generatedTitle;
        Touch(utcNow);
    }

    public bool TryUpdateContextSummary(int assistantSequenceNumber, string summary, DateTime utcNow)
    {
        AiCoachDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (assistantSequenceNumber <= LastSummarySequenceNumber)
            return false;

        ContextSummary = AiCoachDomainRules.NormalizeRequired(summary,
            AiCoachInteractionLimits.MaximumThreadSummaryLength, "Thread summary");
        LastSummarySequenceNumber = assistantSequenceNumber;
        Touch(utcNow);
        return true;
    }

    private void Touch(DateTime utcNow)
    {
        LastActivityAt = utcNow;
        UpdatedAt = utcNow;
        ConcurrencyToken = Guid.NewGuid();
    }
}
