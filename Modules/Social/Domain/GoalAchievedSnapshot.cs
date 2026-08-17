namespace backend.Modules.Social.Domain;

public class GoalAchievedSnapshot
{
    public Guid SourceGoalId { get; private set; }
    public string GoalTypeName { get; private set; } = null!;
    public double TargetValue { get; private set; }
    public string Unit { get; private set; } = null!;
    public DateTime CompletedAt { get; private set; }

    private GoalAchievedSnapshot() { }

    public GoalAchievedSnapshot(
        Guid sourceGoalId,
        string goalTypeName,
        double targetValue,
        string unit,
        DateTime completedAt)
    {
        if (sourceGoalId == Guid.Empty)
            throw new ArgumentException("Source goal id is required.", nameof(sourceGoalId));
        if (string.IsNullOrWhiteSpace(goalTypeName))
            throw new ArgumentException("Goal type name is required.", nameof(goalTypeName));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Goal unit is required.", nameof(unit));

        SourceGoalId = sourceGoalId;
        GoalTypeName = goalTypeName.Trim();
        TargetValue = targetValue;
        Unit = unit.Trim();
        CompletedAt = completedAt;
    }
}
