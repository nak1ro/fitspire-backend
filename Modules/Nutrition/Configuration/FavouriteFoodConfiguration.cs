using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Nutrition.Configuration;

public class FavouriteFoodConfiguration : IEntityTypeConfiguration<FavouriteFood>
{
    public void Configure(EntityTypeBuilder<FavouriteFood> builder)
    {
        builder.ToTable("FavouriteFood");
        builder.HasKey(food => food.Id);
        builder.Property(food => food.Name).HasMaxLength(200).IsRequired();
        builder.Property(food => food.DefinitionKey).HasMaxLength(1_500).IsRequired();
        builder.Property(food => food.Quantity).HasPrecision(12, 3).IsRequired();
        builder.Property(food => food.QuantityUnit).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(food => food.CustomUnitName).HasMaxLength(50);
        builder.Property(food => food.CaloriesKcal).HasPrecision(12, 2);
        builder.Property(food => food.ProteinGrams).HasPrecision(12, 2);
        builder.Property(food => food.CarbsGrams).HasPrecision(12, 2);
        builder.Property(food => food.FatGrams).HasPrecision(12, 2);
        builder.Property(food => food.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(food => new { food.UserId, food.DefinitionKey }).IsUnique()
            .HasDatabaseName("UX_FavouriteFood_ActiveUserDefinition")
            .HasFilter("\"DeletedAt\" IS NULL");
        builder.HasIndex(food => new { food.UserId, food.Name }).HasDatabaseName("IX_FavouriteFood_UserId_Name");
        builder.HasOne(food => food.User).WithMany(user => user.FavouriteFoods)
            .HasForeignKey(food => food.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
