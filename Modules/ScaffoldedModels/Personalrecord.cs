using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Personalrecord
{
    public Guid Id { get; set; }

    public Guid Userid { get; set; }

    public string Workouttype { get; set; } = null!;

    public string Metric { get; set; } = null!;

    public double Value { get; set; }

    public Guid Workoutid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Workout Workout { get; set; } = null!;
}
