using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class GymWorkoutSetConfiguration : IEntityTypeConfiguration<GymWorkoutSet>
{
    public void Configure(EntityTypeBuilder<GymWorkoutSet> builder)
    {
        builder.ToTable("GymWorkoutSet");
        builder.HasKey(set => set.Id);

        builder.Property(set => set.Notes).HasMaxLength(500);
        builder.Property(set => set.CreatedAt).HasDefaultValueSql("NOW()");
        builder.HasIndex(set => new { set.GymWorkoutExerciseId, set.OrderIndex }).IsUnique();

        builder.HasOne(set => set.GymWorkoutExercise)
            .WithMany(exercise => exercise.WorkoutSets)
            .HasForeignKey(set => set.GymWorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
