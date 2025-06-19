using backend.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Moderation.Configuration;

public class BanConfiguration : IEntityTypeConfiguration<Ban>
{
    public void Configure(EntityTypeBuilder<Ban> builder)
    {
        builder.ToTable("Ban");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Reason)
            .IsRequired();

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bans)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}