using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public class SwimmingUserWorkoutDetails : UserWorkout
{
    public int? Laps { get; private set; }
    public double? PoolLengthMeters { get; private set; }
    public double? DistanceMeters { get; private set; }
    public SwimmingStroke? StrokeType { get; private set; }

    // EF Core constructor
    private SwimmingUserWorkoutDetails() { }

    public SwimmingUserWorkoutDetails(
        Guid id,
        Guid userId,
        DateTime date,
        double? durationMinutes
    ) : base(id, userId, "swimming", date)
    {
        if (durationMinutes.HasValue)
            Complete(durationMinutes);
    }

    public void SetPoolDetails(int? laps, double? poolLengthMeters)
    {
        if (laps.HasValue && laps.Value < 0)
            throw new DomainException("Laps cannot be negative.");
            
        if (poolLengthMeters.HasValue && poolLengthMeters.Value <= 0)
            throw new DomainException("Pool length must be positive.");

        Laps = laps;
        PoolLengthMeters = poolLengthMeters;

        // Auto-calculate distance if not set and both values are present
        if (laps.HasValue && poolLengthMeters.HasValue && !DistanceMeters.HasValue)
        {
            DistanceMeters = laps.Value * poolLengthMeters.Value;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDistance(double? distanceMeters)
    {
        if (distanceMeters.HasValue && distanceMeters.Value < 0)
            throw new DomainException("Distance cannot be negative.");

        DistanceMeters = distanceMeters;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStrokeType(SwimmingStroke? strokeType)
    {
        StrokeType = strokeType;
        UpdatedAt = DateTime.UtcNow;
    }
    public override double? GetTotalDistance() => DistanceMeters.HasValue ? DistanceMeters.Value / 1000.0 : null;
}