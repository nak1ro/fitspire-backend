using backend.Modules.Badge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Badge.Configuration;

public class BadgeConfiguration : IEntityTypeConfiguration<AchievementBadge>
{
    public void Configure(EntityTypeBuilder<AchievementBadge> builder)
    {
        builder.ToTable("Badge");

        builder.HasKey(badge => badge.Id);
        builder.Property(badge => badge.Name).HasMaxLength(160).IsRequired();
        builder.Property(badge => badge.Description).HasMaxLength(500);
        builder.Property(badge => badge.IconUrl).HasMaxLength(500);
        builder.Property(badge => badge.Code).HasMaxLength(80).IsRequired();
        builder.Property(badge => badge.Category).HasMaxLength(32).IsRequired();
        builder.Property(badge => badge.SeriesCode).HasMaxLength(80);
        builder.Property(badge => badge.Tier).HasMaxLength(16).IsRequired();
        builder.Property(badge => badge.CriterionCode).HasMaxLength(80).IsRequired();
        builder.Property(badge => badge.MetricCode).HasMaxLength(80);
        builder.Property(badge => badge.CanonicalUnit).HasMaxLength(32).IsRequired();
        builder.Property(badge => badge.ShowProgressWhenLocked).HasDefaultValue(true);
        builder.HasIndex(badge => badge.Code).IsUnique();

        builder.HasMany(badge => badge.UserBadges)
            .WithOne(award => award.AchievementBadge)
            .HasForeignKey(award => award.BadgeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
