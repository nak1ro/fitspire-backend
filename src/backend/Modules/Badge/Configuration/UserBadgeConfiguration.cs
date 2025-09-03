using backend.Modules.Badge.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Badge.Configuration;

public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        builder.ToTable("UserBadge");

        builder.HasKey(ub => ub.Id);

        builder.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();

        builder.Property(ub => ub.AwardedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.Badges)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.AchievementBadge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}