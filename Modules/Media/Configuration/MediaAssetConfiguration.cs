using backend.Modules.Media.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Media.Configuration;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAsset");
        builder.HasKey(asset => asset.Id);

        builder.Property(asset => asset.Purpose).HasConversion<string>().IsRequired();
        builder.Property(asset => asset.Status).HasConversion<string>().IsRequired();
        builder.Property(asset => asset.ClientRequestId).HasMaxLength(128);
        builder.Property(asset => asset.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(asset => asset.DeclaredContentType).HasMaxLength(100).IsRequired();
        builder.Property(asset => asset.StagingBlobKey).HasMaxLength(500).IsRequired();
        builder.Property(asset => asset.UploadedETag).HasMaxLength(128);
        builder.Property(asset => asset.FailureReason).HasMaxLength(500);
        builder.Property(asset => asset.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(asset => asset.UpdatedAt).IsRequired(false);

        builder.HasIndex(asset => new { asset.OwnerUserId, asset.ClientRequestId })
            .IsUnique()
            .HasFilter("\"ClientRequestId\" IS NOT NULL");
        builder.HasIndex(asset => new { asset.OwnerUserId, asset.Status, asset.PendingExpiresAtUtc });
        builder.HasIndex(asset => new { asset.Status, asset.NextCleanupAttemptAtUtc });
        builder.HasIndex(asset => asset.StagingBlobKey).IsUnique();

        builder.HasOne(asset => asset.OwnerUser)
            .WithMany(user => user.MediaAssets)
            .HasForeignKey(asset => asset.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(asset => asset.Variants)
            .WithOne(variant => variant.MediaAsset)
            .HasForeignKey(variant => variant.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
