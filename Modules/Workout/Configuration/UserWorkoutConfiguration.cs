using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Workout.Configuration;

public class UserWorkoutConfiguration : IEntityTypeConfiguration<UserWorkout>
{
    public void Configure(EntityTypeBuilder<UserWorkout> builder)
    {
        builder.ToTable("UserWorkout");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WorkoutType).IsRequired();
        builder.Property(w => w.Date).IsRequired();
        builder.Property(w => w.StartedAt);
        builder.Property(w => w.PausedAt);
        builder.Property(w => w.AccumulatedPausedSeconds).HasDefaultValue(0);
        builder.Property(w => w.DeletedAt);
        builder.Property(w => w.IsPrivate).HasDefaultValue(false);
        builder.Property(w => w.CreatedFromRoutineId);
        builder.Property(w => w.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(w => w.User)
            .WithMany(u => u.Workouts)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.UserId)
            .HasFilter("\"DeletedAt\" IS NULL AND \"Status\" IN (0, 2)")
            .HasDatabaseName("IX_UserWorkout_OneActiveSessionPerUser")
            .IsUnique();
    }
}
