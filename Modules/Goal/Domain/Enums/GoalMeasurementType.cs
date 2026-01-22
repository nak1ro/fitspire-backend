namespace backend.Modules.Goal.Domain.Enums;

public enum GoalMeasurementType
{
    Cumulative,    // Sum over period (e.g., "Run 100km this month")
    SingleEvent,   // One-time achievement (e.g., "Bench 100kg")
    Threshold,     // Maintain above/below value (e.g., "Stay under 80kg")
    Streak         // Consecutive days (e.g., "Workout 5 days in a row")
}
