using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Nutrition.Configuration;

public class CommonFoodConfiguration : IEntityTypeConfiguration<CommonFood>
{
    public void Configure(EntityTypeBuilder<CommonFood> builder)
    {
        builder.ToTable("CommonFood");
        builder.HasKey(food => food.Id);
        builder.Property(food => food.Code).HasMaxLength(100).IsRequired();
        builder.Property(food => food.Name).HasMaxLength(200).IsRequired();
        builder.Property(food => food.Category).HasMaxLength(50).IsRequired();
        builder.Property(food => food.Quantity).HasPrecision(12, 3).IsRequired();
        builder.Property(food => food.QuantityUnit).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(food => food.CustomUnitName).HasMaxLength(50);
        builder.Property(food => food.CaloriesKcal).HasPrecision(12, 2);
        builder.Property(food => food.ProteinGrams).HasPrecision(12, 2);
        builder.Property(food => food.CarbsGrams).HasPrecision(12, 2);
        builder.Property(food => food.FatGrams).HasPrecision(12, 2);
        builder.Property(food => food.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(food => food.Code).IsUnique().HasDatabaseName("UX_CommonFood_Code");
        builder.HasIndex(food => food.Name).HasDatabaseName("IX_CommonFood_Name");
        builder.HasIndex(food => new { food.Category, food.DisplayOrder }).HasDatabaseName("IX_CommonFood_Category_DisplayOrder");
    }
}
