using backend.Modules.User.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Challenge.Domain;

public class UserChallenge
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public Guid CreatedBy { get; private set; }
    public string MetricCode { get; private set; } = null!;
    public string? WorkoutType { get; private set; }
    public string Mode { get; private set; } = ChallengeModes.Target;
    public double? TargetValue { get; private set; }
    public string Visibility { get; private set; } = ChallengeVisibilities.Public;
    public string JoinClosing { get; private set; } = ChallengeJoinClosingModes.AtStart;
    public int ParticipantLimit { get; private set; } = 100;
    public string Status { get; private set; } = ChallengeStatuses.Upcoming;
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation
    public AppUser CreatedByUser { get; set; } = null!;
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
    public ICollection<ChallengeInvitation> Invitations { get; set; } = new List<ChallengeInvitation>();
    public ICollection<ChallengeResult> Results { get; set; } = new List<ChallengeResult>();

    private UserChallenge() { }

    public static UserChallenge Create(Guid creatorId, string title, string? description, string metricCode,
        string? workoutType, string mode, double? targetValue, string visibility, DateTime startDate,
        DateTime endDate, string joinClosing, int participantLimit, DateTime nowUtc)
    {
        ValidateRules(title, metricCode, mode, targetValue, visibility, startDate, endDate, joinClosing, participantLimit, nowUtc, false);

        return new UserChallenge
        {
            Id = Guid.NewGuid(),
            CreatedBy = creatorId,
            Title = title.Trim(),
            Description = NormalizeDescription(description),
            MetricCode = metricCode,
            WorkoutType = NormalizeWorkoutType(workoutType),
            Mode = mode,
            TargetValue = mode == ChallengeModes.Target ? targetValue : null,
            Visibility = visibility,
            StartDate = startDate,
            EndDate = endDate,
            JoinClosing = joinClosing,
            ParticipantLimit = participantLimit,
            Status = startDate <= nowUtc ? ChallengeStatuses.Active : ChallengeStatuses.Upcoming
        };
    }

    public void UpdateBeforeStart(string title, string? description, string metricCode, string? workoutType,
        string mode, double? targetValue, string visibility, DateTime startDate, DateTime endDate,
        string joinClosing, int participantLimit, int activeParticipantCount, DateTime nowUtc)
    {
        if (Status != ChallengeStatuses.Upcoming)
            throw new DomainException("Only upcoming challenges can be edited.");
        if (participantLimit < activeParticipantCount)
            throw new DomainException("Participant limit cannot be lower than the active participant count.");
        if (startDate <= nowUtc)
            throw new DomainException("An edited challenge must keep a future start time.");

        ValidateRules(title, metricCode, mode, targetValue, visibility, startDate, endDate, joinClosing, participantLimit, nowUtc, false);
        Title = title.Trim();
        Description = NormalizeDescription(description);
        MetricCode = metricCode;
        WorkoutType = NormalizeWorkoutType(workoutType);
        Mode = mode;
        TargetValue = mode == ChallengeModes.Target ? targetValue : null;
        Visibility = visibility;
        StartDate = startDate;
        EndDate = endDate;
        JoinClosing = joinClosing;
        ParticipantLimit = participantLimit;
    }

    public void Start(DateTime nowUtc)
    {
        if (Status != ChallengeStatuses.Upcoming || StartDate > nowUtc)
            return;

        Status = ChallengeStatuses.Active;
    }

    public void BeginFinalization(DateTime nowUtc)
    {
        if (Status != ChallengeStatuses.Active || EndDate > nowUtc)
            throw new DomainException("Only ended active challenges can be finalized.");

        Status = ChallengeStatuses.Finalizing;
    }

    public void Complete(DateTime nowUtc)
    {
        if (Status != ChallengeStatuses.Finalizing)
            throw new DomainException("Only finalizing challenges can be completed.");

        Status = ChallengeStatuses.Completed;
        CompletedAt = nowUtc;
    }

    public void Cancel(DateTime nowUtc)
    {
        if (Status is ChallengeStatuses.Completed or ChallengeStatuses.Cancelled)
            return;

        Status = ChallengeStatuses.Cancelled;
        CancelledAt = nowUtc;
    }

    public bool IsJoinOpen(DateTime nowUtc) =>
        (Status is ChallengeStatuses.Upcoming or ChallengeStatuses.Active) &&
        !(JoinClosing == ChallengeJoinClosingModes.AtStart && nowUtc >= StartDate) &&
        nowUtc < EndDate;

    private static void ValidateRules(string title, string metricCode, string mode, double? targetValue,
        string visibility, DateTime startDate, DateTime endDate, string joinClosing, int participantLimit,
        DateTime nowUtc, bool allowImmediatePastStart)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Challenge title is required.");
        if (string.IsNullOrWhiteSpace(metricCode)) throw new DomainException("Challenge metric is required.");
        if (mode is not (ChallengeModes.Target or ChallengeModes.Leaderboard)) throw new DomainException("Challenge mode is invalid.");
        if (mode == ChallengeModes.Target && targetValue is not > 0) throw new DomainException("Target challenges require a positive target.");
        if (visibility is not (ChallengeVisibilities.Public or ChallengeVisibilities.FollowersOnly or ChallengeVisibilities.InviteOnly)) throw new DomainException("Challenge visibility is invalid.");
        if (joinClosing is not (ChallengeJoinClosingModes.AtStart or ChallengeJoinClosingModes.AtEnd)) throw new DomainException("Challenge join closing mode is invalid.");
        if (participantLimit is < 2 or > 100) throw new DomainException("Challenge participant limit must be between two and one hundred.");
        if (!allowImmediatePastStart && startDate < nowUtc.AddMinutes(-1)) throw new DomainException("Challenge start time cannot be in the past.");
        if (endDate < startDate.AddDays(1) || endDate > startDate.AddYears(1)) throw new DomainException("Challenge duration must be between one day and one year.");
    }

    private static string? NormalizeDescription(string? description) => string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    private static string? NormalizeWorkoutType(string? workoutType) => string.IsNullOrWhiteSpace(workoutType) ? null : workoutType.Trim().ToLowerInvariant();
}
