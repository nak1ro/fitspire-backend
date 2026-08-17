using backend.Modules.Goal.Domain.Constants;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Services;

public interface IGoalTemplatePolicy
{
    GoalCreationRules Resolve(GoalType template, string schedule, DateTime? deadline, string? selectedWorkoutType,
        Guid? selectedExerciseId, DateTime? startDate);
}

public sealed record GoalCreationRules(
    bool IsRecurring,
    string? RecurrencePattern,
    DateTime? Deadline,
    string? SelectedWorkoutType,
    Guid? SelectedExerciseId,
    DateTime StartDate);

public class GoalTemplatePolicy : IGoalTemplatePolicy
{
    private static readonly HashSet<string> WorkoutTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "gym", "running", "cycling", "swimming", "yoga"
    };

    /// <summary>
    /// <paramref name="deadline"/> and <paramref name="startDate"/> must already be resolved to UTC
    /// (via <see cref="backend.Modules.Shared.Service.IUserLocalDateResolver"/>) before calling this —
    /// this method no longer performs its own UTC conversion, since <c>DateTime.ToUniversalTime()</c>
    /// on an unspecified-kind value silently converts using the server's local timezone rather than
    /// the user's saved preference.
    /// </summary>
    public GoalCreationRules Resolve(GoalType template, string schedule, DateTime? deadline, string? selectedWorkoutType,
        Guid? selectedExerciseId, DateTime? startDate)
    {
        if (!template.IsActive || string.IsNullOrWhiteSpace(template.MetricCode))
            throw new DomainException("This goal template is not available.");

        var normalizedSchedule = NormalizeSchedule(schedule);
        var resolvedStartDate = startDate ?? DateTime.UtcNow;
        ValidateSchedule(template, normalizedSchedule, deadline, resolvedStartDate);
        var workoutType = ResolveWorkoutType(template, selectedWorkoutType);
        ValidateExercise(template, selectedExerciseId);
        return new GoalCreationRules(normalizedSchedule != GoalSchedules.OneOff,
            normalizedSchedule == GoalSchedules.OneOff ? null : normalizedSchedule,
            normalizedSchedule == GoalSchedules.OneOff ? deadline!.Value : null,
            workoutType, selectedExerciseId, resolvedStartDate);
    }

    public static IReadOnlyList<string> GetAllowedSchedules(GoalType template) =>
        template.MeasurementType == GoalMeasurementType.Cumulative
            ? [GoalSchedules.OneOff, GoalSchedules.Daily, GoalSchedules.Weekly, GoalSchedules.Monthly]
            : [GoalSchedules.OneOff];

    private static string NormalizeSchedule(string schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule) || !GoalSchedules.All.Contains(schedule))
            throw new DomainException("Goal schedule must be one-off, daily, weekly, or monthly.");
        return schedule.Trim().ToLowerInvariant();
    }

    private static void ValidateSchedule(GoalType template, string schedule, DateTime? deadline, DateTime startDate)
    {
        if (schedule != GoalSchedules.OneOff && template.MeasurementType != GoalMeasurementType.Cumulative)
            throw new DomainException("Only cumulative goal templates can recur.");
        if (startDate < DateTime.UtcNow.AddMinutes(-1))
            throw new DomainException("Goal start date cannot be in the past.");
        if (schedule == GoalSchedules.OneOff && (!deadline.HasValue || deadline.Value <= DateTime.UtcNow || deadline.Value <= startDate))
            throw new DomainException("One-off goals require a future deadline.");
        if (schedule != GoalSchedules.OneOff && deadline.HasValue)
            throw new DomainException("Recurring goals do not use an overall deadline.");
    }

    private static string? ResolveWorkoutType(GoalType template, string? selectedWorkoutType)
    {
        var intrinsicWorkoutType = template.RelatedWorkoutType?.ToLowerInvariant();
        if (intrinsicWorkoutType is not null)
        {
            if (!string.IsNullOrWhiteSpace(selectedWorkoutType) && !string.Equals(selectedWorkoutType, intrinsicWorkoutType, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("This goal template already defines its workout type.");
            return intrinsicWorkoutType;
        }

        if (template.ParameterKind != "WorkoutType")
        {
            if (!string.IsNullOrWhiteSpace(selectedWorkoutType))
                throw new DomainException("This goal template does not accept a workout type filter.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(selectedWorkoutType))
            return null;
        if (!WorkoutTypes.Contains(selectedWorkoutType))
            throw new DomainException("Goal workout type is not supported.");
        return selectedWorkoutType.Trim().ToLowerInvariant();
    }

    private static void ValidateExercise(GoalType template, Guid? selectedExerciseId)
    {
        if (template.ParameterKind == "Exercise" && !selectedExerciseId.HasValue)
            throw new DomainException("This goal template requires an exercise.");
        if (template.ParameterKind != "Exercise" && selectedExerciseId.HasValue)
            throw new DomainException("This goal template does not accept an exercise.");
    }
}

public static class GoalDefinitionKeyFactory
{
    public static string Create(GoalType template, string schedule, string? selectedWorkoutType, Guid? selectedExerciseId) =>
        string.Join('|', template.Code, schedule, selectedWorkoutType ?? "any", selectedExerciseId?.ToString("N") ?? "none");
}
