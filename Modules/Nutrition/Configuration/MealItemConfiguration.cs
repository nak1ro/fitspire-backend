using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Nutrition.Configuration;

public class MealItemConfiguration : IEntityTypeConfiguration<MealItem>
{
    public void Configure(EntityTypeBuilder<MealItem> builder)
    {
        builder.ToTable("MealItem");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Name).HasMaxLength(200).IsRequired();
        builder.Property(mi => mi.Quantity).HasPrecision(12, 3).IsRequired();
        builder.Property(mi => mi.QuantityUnit).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(mi => mi.CustomUnitName).HasMaxLength(50);
        builder.Property(mi => mi.CaloriesKcal).HasPrecision(12, 2);
        builder.Property(mi => mi.ProteinGrams).HasPrecision(12, 2);
        builder.Property(mi => mi.CarbsGrams).HasPrecision(12, 2);
        builder.Property(mi => mi.FatGrams).HasPrecision(12, 2);
        builder.Property(mi => mi.SnapshotKey).HasMaxLength(1_500).IsRequired();
        builder.Property(mi => mi.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(mi => new { mi.MealId, mi.OrderIndex })
            .IsUnique()
            .HasDatabaseName("UX_MealItem_ActiveMealOrder")
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasOne<FavouriteFood>()
            .WithMany()
            .HasForeignKey(mi => mi.FavouriteFoodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
