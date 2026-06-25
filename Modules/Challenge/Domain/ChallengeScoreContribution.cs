namespace backend.Modules.Challenge.Domain;

public class ChallengeScoreContribution
{
    public Guid Id { get; set; }
    public Guid ChallengeId { get; set; }
    public Guid ParticipantId { get; set; }
    public Guid ActivityContributionId { get; set; }
    public double Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
