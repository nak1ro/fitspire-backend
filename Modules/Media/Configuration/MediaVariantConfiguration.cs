using backend.Modules.Media.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Media.Configuration;

public class MediaVariantConfiguration : IEntityTypeConfiguration<MediaVariant>
{
    public void Configure(EntityTypeBuilder<MediaVariant> builder)
    {
        builder.ToTable("MediaVariant");
        builder.HasKey(variant => variant.Id);

        builder.Property(variant => variant.Kind).HasConversion<string>().IsRequired();
        builder.Property(variant => variant.BlobKey).HasMaxLength(500).IsRequired();
        builder.Property(variant => variant.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(variant => variant.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(variant => new { variant.MediaAssetId, variant.Kind }).IsUnique();
        builder.HasIndex(variant => variant.BlobKey).IsUnique();
    }
}
