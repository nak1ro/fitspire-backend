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

        builder.Property(mi => mi.Name)
            .IsRequired();

        builder.Property(mi => mi.Calories);
        builder.Property(mi => mi.Protein);
        builder.Property(mi => mi.Carbs);
        builder.Property(mi => mi.Fat);
        builder.Property(mi => mi.Quantity);
        builder.Property(mi => mi.Unit);
    }
}