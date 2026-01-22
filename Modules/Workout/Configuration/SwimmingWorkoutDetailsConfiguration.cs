using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class SwimmingUserWorkoutDetailsConfiguration : IEntityTypeConfiguration<SwimmingUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<SwimmingUserWorkoutDetails> builder)
    {
        builder.Property(x => x.Laps);
        
        builder.Property(x => x.PoolLengthMeters);

        builder.Property(x => x.DistanceMeters);

        builder.Property(x => x.StrokeType)
            .HasMaxLength(100);
    }
}