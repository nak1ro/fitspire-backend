using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class YogaUserWorkoutDetailsConfiguration : IEntityTypeConfiguration<YogaUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<YogaUserWorkoutDetails> builder)
    {
        builder.Property(x => x.Style)
            .HasConversion<string>();

        builder.Property(x => x.Intensity)
            .HasConversion<string>();

        builder.Property(x => x.FocusArea)
            .HasConversion<string>();
    }
}