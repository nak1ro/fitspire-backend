using backend.Modules.Challenge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Challenge.Configuration;

public class ChallengeParticipantConfiguration : IEntityTypeConfiguration<ChallengeParticipant>
{
    public void Configure(EntityTypeBuilder<ChallengeParticipant> builder)
    {
        builder.ToTable("ChallengeParticipant");

        builder.HasKey(cp => cp.Id);

        builder.HasIndex(cp => new { cp.ChallengeId, cp.UserId }).IsUnique();

        builder.HasOne(cp => cp.User)
            .WithMany(u => u.ChallengeParticipants)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.UserChallenge)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}