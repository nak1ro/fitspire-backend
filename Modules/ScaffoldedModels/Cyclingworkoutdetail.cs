using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Cyclingworkoutdetail
{
    public Guid Id { get; set; }

    public Guid Workoutid { get; set; }

    public double? Distancekm { get; set; }

    public double? Elevationgain { get; set; }

    public double? Avgspeedkmperhour { get; set; }

    public virtual Workout Workout { get; set; } = null!;
}
