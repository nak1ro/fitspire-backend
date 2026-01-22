using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class RunningUserWorkoutDetailsConfiguration : IEntityTypeConfiguration<RunningUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<RunningUserWorkoutDetails> builder)
    {
        builder.HasBaseType<UserWorkout>();

        builder.Property(w => w.DistanceKm).IsRequired();
        builder.Property(w => w.ElevationGainMeters);
        builder.Property(w => w.StepCount);
        builder.Property(w => w.MapData).HasMaxLength(4000); // Plenty for simplified polyline
    }
}
