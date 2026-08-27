using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Post");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Content)
            .HasMaxLength(2000);
        
        builder.Property(p => p.ReferenceEntityId);
        builder.Property(p => p.ModerationRemovedAtUtc);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");
        builder.HasIndex(p => new { p.UserId, p.CreatedAt });

        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(p => p.WorkoutShareSnapshot, snapshot =>
        {
            snapshot.Property(value => value.SourceWorkoutId)
                .HasColumnName("SourceWorkoutId");
            snapshot.Property(value => value.WorkoutType)
                .HasColumnName("SharedWorkoutType")
                .HasMaxLength(64);
            snapshot.Property(value => value.WorkoutDate)
                .HasColumnName("SharedWorkoutDate");
            snapshot.Property(value => value.DurationMinutes)
                .HasColumnName("SharedDurationMinutes");
            snapshot.Property(value => value.DistanceKm)
                .HasColumnName("SharedDistanceKm");
            snapshot.Property(value => value.CaloriesBurned)
                .HasColumnName("SharedCaloriesBurned");
            snapshot.Property(value => value.TotalVolumeKg)
                .HasColumnName("SharedTotalVolumeKg");
            snapshot.Property(value => value.ExerciseCount)
                .HasColumnName("SharedExerciseCount");
            snapshot.Property(value => value.CompletedAt)
                .HasColumnName("SharedCompletedAt");

            snapshot.HasIndex(value => value.SourceWorkoutId)
                .IsUnique()
                .HasFilter("\"SourceWorkoutId\" IS NOT NULL");
        });

        builder.OwnsOne(p => p.GoalAchievedSnapshot, snapshot =>
        {
            snapshot.Property(value => value.SourceGoalId)
                .HasColumnName("SourceGoalId");
            snapshot.Property(value => value.GoalTypeName)
                .HasColumnName("SharedGoalTypeName")
                .HasMaxLength(64);
            snapshot.Property(value => value.TargetValue)
                .HasColumnName("SharedGoalTargetValue");
            snapshot.Property(value => value.Unit)
                .HasColumnName("SharedGoalUnit")
                .HasMaxLength(32);
            snapshot.Property(value => value.CompletedAt)
                .HasColumnName("SharedGoalCompletedAt");

            snapshot.HasIndex(value => value.SourceGoalId)
                .IsUnique()
                .HasFilter("\"SourceGoalId\" IS NOT NULL");
        });

        builder.OwnsOne(p => p.PersonalRecordAchievedSnapshot, snapshot =>
        {
            snapshot.Property(value => value.SourcePersonalRecordId)
                .HasColumnName("SourcePersonalRecordId");
            snapshot.Property(value => value.WorkoutType)
                .HasColumnName("SharedPersonalRecordWorkoutType")
                .HasMaxLength(64);
            snapshot.Property(value => value.Metric)
                .HasColumnName("SharedPersonalRecordMetric")
                .HasMaxLength(64);
            snapshot.Property(value => value.ExerciseId)
                .HasColumnName("SharedPersonalRecordExerciseId");
            snapshot.Property(value => value.ExerciseName)
                .HasColumnName("SharedPersonalRecordExerciseName")
                .HasMaxLength(128);
            snapshot.Property(value => value.Value)
                .HasColumnName("SharedPersonalRecordValue");
            snapshot.Property(value => value.Unit)
                .HasColumnName("SharedPersonalRecordUnit")
                .HasMaxLength(32);
            snapshot.Property(value => value.AchievedAt)
                .HasColumnName("SharedPersonalRecordAchievedAt");

            // Composite, unlike Workout/Goal — a PersonalRecord row is mutable (same Id,
            // Value/AchievedAt overwritten each time it's broken again), so uniqueness is
            // scoped to a specific achievement moment, not the record itself, letting a
            // future break become shareable again.
            snapshot.HasIndex(value => new { value.SourcePersonalRecordId, value.AchievedAt })
                .IsUnique()
                .HasFilter("\"SourcePersonalRecordId\" IS NOT NULL");
        });

        builder.HasMany(p => p.SavedByUsers)
            .WithOne(sp => sp.Post)
            .HasForeignKey(sp => sp.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
