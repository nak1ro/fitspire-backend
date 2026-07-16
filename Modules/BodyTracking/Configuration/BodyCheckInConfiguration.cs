using backend.Modules.BodyTracking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.BodyTracking.Configuration;

public class BodyCheckInConfiguration : IEntityTypeConfiguration<BodyCheckIn>
{
    public void Configure(EntityTypeBuilder<BodyCheckIn> builder)
    {
        builder.ToTable("BodyCheckIn");
        builder.HasKey(checkIn => checkIn.Id);

        builder.Property(checkIn => checkIn.CheckInDate).IsRequired();
        builder.Property(checkIn => checkIn.WeightKg).HasPrecision(8, 2);
        builder.Property(checkIn => checkIn.BodyFatPercent).HasPrecision(5, 2);
        builder.Property(checkIn => checkIn.WaistCm).HasPrecision(6, 2);
        builder.Property(checkIn => checkIn.ChestCm).HasPrecision(6, 2);
        builder.Property(checkIn => checkIn.HipsCm).HasPrecision(6, 2);
        builder.Property(checkIn => checkIn.ArmCm).HasPrecision(6, 2);
        builder.Property(checkIn => checkIn.ThighCm).HasPrecision(6, 2);
        builder.Property(checkIn => checkIn.Note).HasMaxLength(1_000);
        builder.Property(checkIn => checkIn.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(checkIn => new { checkIn.UserId, checkIn.CheckInDate })
            .IsUnique()
            .HasDatabaseName("UX_BodyCheckIn_ActiveUserDate")
            .HasFilter("\"DeletedAt\" IS NULL");
        builder.HasIndex(checkIn => checkIn.PhotoMediaId).IsUnique().HasFilter("\"PhotoMediaId\" IS NOT NULL");

        builder.HasOne(checkIn => checkIn.User)
            .WithMany(user => user.BodyCheckIns)
            .HasForeignKey(checkIn => checkIn.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(checkIn => checkIn.PhotoMedia)
            .WithOne()
            .HasForeignKey<BodyCheckIn>(checkIn => checkIn.PhotoMediaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
