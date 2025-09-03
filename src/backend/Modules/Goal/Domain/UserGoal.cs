using backend.Modules.User.Domain;

namespace backend.Modules.Goal.Domain;

public class UserGoal
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public Guid GoalTypeId { get; set; }

    public float TargetValue { get; set; }
    public float? CurrentValue { get; set; }
    public string Unit { get; set; } = null!;
    public DateTime? Deadline { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
    public GoalType GoalType { get; set; } = null!;
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();
}