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

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");

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

        builder.HasMany(p => p.SavedByUsers)
            .WithOne(sp => sp.Post)
            .HasForeignKey(sp => sp.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
