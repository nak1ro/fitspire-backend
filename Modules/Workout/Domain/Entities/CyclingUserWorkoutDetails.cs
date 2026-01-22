using backend.Modules.Workout.Domain.Enums;

namespace backend.Modules.Workout.Domain.Entities;

public class CyclingUserWorkoutDetails : UserWorkout
{
    public double DistanceKm { get; private set; }
    public double? ElevationGainMeters { get; private set; }
    public string? MapData { get; private set; }
    public bool IsIndoor { get; private set; }

    // EF Core constructor
    private CyclingUserWorkoutDetails() { }

    public CyclingUserWorkoutDetails(
        Guid id,
        Guid userId,
        DateTime date,
        double distanceKm,
        double? durationMinutes,
        bool isIndoor
    ) : base(id, userId, "cycling", date)
    {
        if (distanceKm <= 0)
            throw new ArgumentException("Distance must be greater than 0", nameof(distanceKm));

        DistanceKm = distanceKm;
        IsIndoor = isIndoor;
        
        if (durationMinutes.HasValue)
            Complete(durationMinutes);
    }

    public void UpdateStats(double? elevationGainMeters, string? mapData)
    {
        ElevationGainMeters = elevationGainMeters;
        MapData = mapData;
    }
    public override double? GetTotalDistance() => DistanceKm;
}
