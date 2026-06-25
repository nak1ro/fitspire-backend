using backend.Modules.Badge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Badge.Configuration;

public class BadgeConfiguration : IEntityTypeConfiguration<AchievementBadge>
{
    public void Configure(EntityTypeBuilder<AchievementBadge> builder)
    {
        builder.ToTable("Badge");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired();

        builder.Property(b => b.Description);
        builder.Property(b => b.IconUrl);
        builder.Property(b => b.Code).HasMaxLength(80).IsRequired();
        builder.Property(b => b.Category).HasMaxLength(32).IsRequired();
        builder.Property(b => b.SeriesCode).HasMaxLength(80);
        builder.Property(b => b.Tier).HasMaxLength(16).IsRequired();
        builder.Property(b => b.CriterionCode).HasMaxLength(80).IsRequired();
        builder.Property(b => b.MetricCode).HasMaxLength(80);
        builder.HasIndex(b => b.Code).IsUnique();

        builder.HasMany(b => b.UserBadges)
            .WithOne(ub => ub.AchievementBadge)
            .HasForeignKey(ub => ub.BadgeId);
    }
}
