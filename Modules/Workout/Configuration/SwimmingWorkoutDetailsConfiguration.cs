using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class SwimmingWorkoutDetailsConfiguration : IEntityTypeConfiguration<SwimmingUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<SwimmingUserWorkoutDetails> builder)
    {
        builder.ToTable("SwimmingWorkoutDetails");
        
        builder.Property(s => s.Laps);
        builder.Property(s => s.DistanceMeters);
        builder.Property(s => s.StrokeType);
    }
}