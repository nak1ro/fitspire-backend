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

        builder.Property(m => m.Date)
            .IsRequired();

        builder.Property(m => m.TotalCalories);

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(m => m.User)
            .WithMany(u => u.Meals)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Items)
            .WithOne(i => i.Meal)
            .HasForeignKey(i => i.MealId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}