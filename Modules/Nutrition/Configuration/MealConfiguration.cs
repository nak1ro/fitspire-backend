using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Nutrition.Configuration;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("Meal");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MealDate).IsRequired();
        builder.Property(m => m.MealType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(1_000);
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(m => new { m.UserId, m.MealDate })
            .HasDatabaseName("IX_Meal_UserId_MealDate");

        builder.HasOne(m => m.User)
            .WithMany(u => u.Meals)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Items)
            .WithOne(i => i.Meal)
            .HasForeignKey(i => i.MealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
