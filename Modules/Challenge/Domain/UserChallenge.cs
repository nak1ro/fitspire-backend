using backend.Modules.User.Domain;

namespace backend.Modules.Challenge.Domain;

public class UserChallenge
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public Guid CreatedBy { get; set; }
    public string MetricCode { get; set; } = null!;
    public string? WorkoutType { get; set; }
    public string Mode { get; set; } = "Target";
    public double? TargetValue { get; set; }
    public string Visibility { get; set; } = "Public";
    public string JoinClosing { get; set; } = "AtStart";
    public int ParticipantLimit { get; set; } = 100;
    public string Status { get; set; } = "Upcoming";
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public AppUser CreatedByUser { get; set; } = null!;
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
    public ICollection<ChallengeInvitation> Invitations { get; set; } = new List<ChallengeInvitation>();
    public ICollection<ChallengeResult> Results { get; set; } = new List<ChallengeResult>();
}
