namespace backend.Modules.Goal.Domain;

public class GoalType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string? IconUrl { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<UserGoal> Goals { get; set; } = new List<UserGoal>();
}