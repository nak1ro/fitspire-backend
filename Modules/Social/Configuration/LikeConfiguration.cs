using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("Like");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TargetType)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(l => l.User)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.UserId, l.TargetId, l.TargetType })
            .IsUnique();
    }
}