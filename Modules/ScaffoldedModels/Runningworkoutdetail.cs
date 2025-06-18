using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Runningworkoutdetail
{
    public Guid Id { get; set; }

    public Guid Workoutid { get; set; }

    public double? Distancekm { get; set; }

    public double? Avgpaceminperkm { get; set; }

    public virtual Workout Workout { get; set; } = null!;
}
