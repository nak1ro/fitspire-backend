using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Yogaworkoutdetail
{
    public Guid Id { get; set; }

    public Guid Workoutid { get; set; }

    public string? Style { get; set; }

    public string? Intensity { get; set; }

    public string? Focusarea { get; set; }

    public virtual Workout Workout { get; set; } = null!;
}
