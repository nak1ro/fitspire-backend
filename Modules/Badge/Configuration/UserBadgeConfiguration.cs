using backend.Modules.Badge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Badge.Configuration;

public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        builder.ToTable("UserBadge");

        builder.HasKey(award => award.Id);
        builder.HasIndex(award => new { award.UserId, award.BadgeId }).IsUnique();
        builder.HasIndex(award => new { award.UserId, award.FeaturedOrder })
            .HasFilter("\"FeaturedOrder\" IS NOT NULL")
            .IsUnique();

        builder.Property(award => award.AwardedAt)
            .HasDefaultValueSql("NOW()");
        builder.Property(award => award.CriterionCode).HasColumnName("EvidenceType").HasMaxLength(80);
        builder.Property(award => award.CanonicalUnit).HasMaxLength(32);
        builder.Property(award => award.TriggeringEntityType).HasMaxLength(64);
        builder.Property(award => award.EvidenceSummary).HasMaxLength(500);

        builder.HasOne(award => award.User)
            .WithMany(user => user.Badges)
            .HasForeignKey(award => award.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(award => award.AchievementBadge)
            .WithMany(badge => badge.UserBadges)
            .HasForeignKey(award => award.BadgeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
