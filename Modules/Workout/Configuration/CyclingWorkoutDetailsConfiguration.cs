using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class CyclingWorkoutDetailsConfiguration : IEntityTypeConfiguration<CyclingUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<CyclingUserWorkoutDetails> builder)
    {
        builder.ToTable("CyclingWorkoutDetails");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.DistanceKm);
        builder.Property(c => c.ElevationGain);
        builder.Property(c => c.AvgSpeedKmPerHour);
    }
}