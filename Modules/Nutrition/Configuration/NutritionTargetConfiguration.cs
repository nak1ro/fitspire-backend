using backend.Modules.Nutrition.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Nutrition.Configuration;

public class NutritionTargetConfiguration : IEntityTypeConfiguration<NutritionTarget>
{
    public void Configure(EntityTypeBuilder<NutritionTarget> builder)
    {
        builder.ToTable("NutritionTarget");
        builder.HasKey(target => target.Id);
        builder.Property(target => target.CaloriesKcal).HasPrecision(12, 2);
        builder.Property(target => target.ProteinGrams).HasPrecision(12, 2);
        builder.Property(target => target.CarbsGrams).HasPrecision(12, 2);
        builder.Property(target => target.FatGrams).HasPrecision(12, 2);
        builder.Property(target => target.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(target => target.UserId).IsUnique().HasDatabaseName("UX_NutritionTarget_UserId");
        builder.HasOne(target => target.User).WithOne(user => user.NutritionTarget)
            .HasForeignKey<NutritionTarget>(target => target.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
