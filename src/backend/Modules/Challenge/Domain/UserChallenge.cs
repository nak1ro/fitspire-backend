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

    // Navigation
    public AppUser CreatedByUser { get; set; } = null!;
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
}