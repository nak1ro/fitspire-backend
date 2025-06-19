using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class GymWorkoutDetailsConfiguration : IEntityTypeConfiguration<GymUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<GymUserWorkoutDetails> builder)
    {
        builder.ToTable("GymWorkoutDetails");
        
        builder.HasKey(g => g.Id);

        builder.HasMany(g => g.Exercises)
            .WithOne(e => e.GymUserWorkout)
            .HasForeignKey(e => e.GymWorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}