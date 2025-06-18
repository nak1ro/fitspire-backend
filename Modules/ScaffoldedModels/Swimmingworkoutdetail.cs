using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Swimmingworkoutdetail
{
    public Guid Id { get; set; }

    public Guid Workoutid { get; set; }

    public int? Laps { get; set; }

    public double? Distancemeters { get; set; }

    public string? Stroketype { get; set; }

    public virtual Workout Workout { get; set; } = null!;
}
