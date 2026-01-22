using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class CyclingUserWorkoutDetailsConfiguration : IEntityTypeConfiguration<CyclingUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<CyclingUserWorkoutDetails> builder)
    {
        builder.Property(x => x.DistanceKm)
            .IsRequired();

        builder.Property(x => x.ElevationGainMeters);

        builder.Property(x => x.MapData)
            .HasMaxLength(10000); // Allow sufficient space for JSON/polyline data
            
        builder.Property(x => x.IsIndoor)
            .IsRequired();
    }
}