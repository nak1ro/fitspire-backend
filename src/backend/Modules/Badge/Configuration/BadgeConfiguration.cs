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

        builder.HasMany(b => b.UserBadges)
            .WithOne(ub => ub.AchievementBadge)
            .HasForeignKey(ub => ub.BadgeId);
    }
}