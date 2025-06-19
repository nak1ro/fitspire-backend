using backend.Modules.User.Domain;

namespace backend.Modules.Goal.Domain;

public class ProgressEntry
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid? GoalId { get; set; }

    public DateTime Date { get; set; }
    public float? Weight { get; set; }
    public float? BodyFat { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public UserGoal? Goal { get; set; }
}