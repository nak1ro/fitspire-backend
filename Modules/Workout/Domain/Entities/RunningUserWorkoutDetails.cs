using backend.Modules.Shared.Domain;

namespace backend.Modules.Workout.Domain.Entities;

public class RunningUserWorkoutDetails : UserWorkout
{
    public double DistanceKm { get; private set; }
    public double? ElevationGainMeters { get; private set; }
    public int? StepCount { get; private set; }
    public string? MapData { get; private set; } // JSON polylines or simple text for MVP

    // EF Core constructor
    private RunningUserWorkoutDetails() { }

    public RunningUserWorkoutDetails(Guid id, Guid userId, DateTime date, double distanceKm, double? durationMinutes)
        : base(id, userId, "running", date)
    {
        SetDistance(distanceKm);
        
        if (durationMinutes.HasValue)
            Complete(durationMinutes);
    }

    public void SetDistance(double distanceKm)
    {
        if (distanceKm < 0)
            throw new DomainException("Distance cannot be negative.");
            
        DistanceKm = distanceKm;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStats(double? elevationGain, int? steps, string? mapData)
    {
        if (elevationGain.HasValue && elevationGain.Value < 0)
            throw new DomainException("Elevation gain cannot be negative.");
            
        if (steps.HasValue && steps.Value < 0)
            throw new DomainException("Step count cannot be negative.");

        ElevationGainMeters = elevationGain;
        StepCount = steps;
        MapData = mapData;
        UpdatedAt = DateTime.UtcNow;
    }
    public override double? GetTotalDistance() => DistanceKm;
}
