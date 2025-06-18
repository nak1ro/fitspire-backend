using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Gymworkoutdetail
{
    public Guid Id { get; set; }

    public Guid Workoutid { get; set; }

    public string? Splittype { get; set; }

    public string? Intensitylevel { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Gymworkoutexercise> Gymworkoutexercises { get; set; } = new List<Gymworkoutexercise>();

    public virtual Workout Workout { get; set; } = null!;
}
