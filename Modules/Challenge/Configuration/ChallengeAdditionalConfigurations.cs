using backend.Modules.Challenge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Challenge.Configuration;

public class ChallengeInvitationConfiguration : IEntityTypeConfiguration<ChallengeInvitation>
{
    public void Configure(EntityTypeBuilder<ChallengeInvitation> builder)
    {
        builder.ToTable("ChallengeInvitation"); builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasMaxLength(16).IsRequired();
        builder.HasIndex(item => new { item.ChallengeId, item.InvitedUserId }).IsUnique();
        builder.HasOne(item => item.Challenge).WithMany(challenge => challenge.Invitations).HasForeignKey(item => item.ChallengeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.InvitedUser).WithMany().HasForeignKey(item => item.InvitedUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
public class ChallengeScoreContributionConfiguration : IEntityTypeConfiguration<ChallengeScoreContribution>
{
    public void Configure(EntityTypeBuilder<ChallengeScoreContribution> builder)
    {
        builder.ToTable("ChallengeScoreContribution"); builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.ParticipantId, item.ActivityContributionId }).IsUnique();
        builder.HasIndex(item => item.ChallengeId);
    }
}
public class ChallengeResultConfiguration : IEntityTypeConfiguration<ChallengeResult>
{
    public void Configure(EntityTypeBuilder<ChallengeResult> builder)
    {
        builder.ToTable("ChallengeResult"); builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.ChallengeId, item.ParticipantId }).IsUnique();
        builder.HasIndex(item => new { item.UserId, item.FinalizedAt });
        builder.HasOne(item => item.Challenge).WithMany(challenge => challenge.Results).HasForeignKey(item => item.ChallengeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
