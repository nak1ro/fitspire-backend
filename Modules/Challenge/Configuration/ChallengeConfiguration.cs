using backend.Modules.Challenge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Challenge.Configuration;

public class ChallengeConfiguration : IEntityTypeConfiguration<UserChallenge>
{
    public void Configure(EntityTypeBuilder<UserChallenge> builder)
    {
        builder.ToTable("Challenge");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired();

        builder.Property(c => c.Description);

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();
        builder.Property(c => c.MetricCode).HasMaxLength(80).IsRequired();
        builder.Property(c => c.WorkoutType).HasMaxLength(32);
        builder.Property(c => c.Mode).HasMaxLength(16).IsRequired();
        builder.Property(c => c.Visibility).HasMaxLength(16).IsRequired();
        builder.Property(c => c.JoinClosing).HasMaxLength(16).IsRequired();
        builder.Property(c => c.Status).HasMaxLength(16).IsRequired();
        builder.HasIndex(c => new { c.Status, c.StartDate, c.EndDate });

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.ChallengesCreated)
            .HasForeignKey(c => c.CreatedBy);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.UserChallenge)
            .HasForeignKey(p => p.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
