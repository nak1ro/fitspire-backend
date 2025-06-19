using backend.Modules.Workout.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Modules.Workout.Configuration;

public class YogaWorkoutDetailsConfiguration : IEntityTypeConfiguration<YogaUserWorkoutDetails>
{
    public void Configure(EntityTypeBuilder<YogaUserWorkoutDetails> builder)
    {
        builder.ToTable("YogaWorkoutDetails");

        builder.HasKey(y => y.Id);

        builder.Property(y => y.Style);
        builder.Property(y => y.Intensity);
        builder.Property(y => y.FocusArea);
    }
}