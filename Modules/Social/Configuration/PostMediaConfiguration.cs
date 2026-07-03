using backend.Modules.Social.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Social.Configuration;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("PostMedia");
        builder.HasKey(media => media.Id);
        builder.Property(media => media.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(media => new { media.PostId, media.Order }).IsUnique();
        builder.HasIndex(media => media.MediaAssetId).IsUnique();

        builder.HasOne(media => media.Post)
            .WithMany(post => post.Media)
            .HasForeignKey(media => media.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(media => media.MediaAsset)
            .WithOne()
            .HasForeignKey<PostMedia>(media => media.MediaAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
