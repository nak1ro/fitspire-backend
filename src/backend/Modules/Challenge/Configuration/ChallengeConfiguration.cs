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

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.ChallengesCreated)
            .HasForeignKey(c => c.CreatedBy);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.UserChallenge)
            .HasForeignKey(p => p.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}