using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public class YogaUserWorkoutDetails : UserWorkout
{
    public YogaStyle? Style { get; private set; }
    public YogaIntensity? Intensity { get; private set; }
    public YogaFocusArea? FocusArea { get; private set; }

    // EF Core constructor
    private YogaUserWorkoutDetails() { }

    public YogaUserWorkoutDetails(
        Guid id,
        Guid userId,
        DateTime date,
        double? durationMinutes,
        string? notes
    ) : base(id, userId, "yoga", date)
    {
        if (durationMinutes.HasValue)
            Complete(durationMinutes);
            
        if (notes != null) 
            UpdateNotes(notes);
    }

    public void SetDetails(YogaStyle? style, YogaIntensity? intensity, YogaFocusArea? focusArea)
    {
        Style = style;
        Intensity = intensity;
        FocusArea = focusArea;
        UpdatedAt = DateTime.UtcNow;
    }
}