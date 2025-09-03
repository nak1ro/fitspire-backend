using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class SavedPostConfiguration : IEntityTypeConfiguration<SavedPost>
{
    public void Configure(EntityTypeBuilder<SavedPost> builder)
    {
        builder.ToTable("SavedPost");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.SavedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(sp => sp.User)
            .WithMany(u => u.SavedPosts)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Post)
            .WithMany(p => p.SavedByUsers)
            .HasForeignKey(sp => sp.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sp => new { sp.UserId, sp.PostId }).IsUnique();
    }
}