using backend.Modules.User.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.User.Configuration;

public class AppUserPreferenceConfiguration : IEntityTypeConfiguration<AppUserPreference>
{
    public void Configure(EntityTypeBuilder<AppUserPreference> builder)
    {
        builder.ToTable("UserPreference");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PreferredLanguage)
            .HasDefaultValue("en");

        builder.Property(p => p.IsDarkModeEnabled)
            .HasDefaultValue(false);

        builder.Property(p => p.ReceiveEmailNotifications)
            .HasDefaultValue(true);

        builder.Property(p => p.UnitSystem)
            .HasDefaultValue("metric");

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(p => p.AppUser)
            .WithOne(u => u.AppUserPreference)
            .HasForeignKey<AppUserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
