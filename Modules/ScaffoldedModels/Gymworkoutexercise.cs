using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Gymworkoutexercise
{
    public Guid Id { get; set; }

    public Guid Gymworkoutid { get; set; }

    public Guid? Exerciseid { get; set; }

    public int? Sets { get; set; }

    public int? Reps { get; set; }

    public double? Weight { get; set; }

    public double? Durationminutes { get; set; }

    public int? Orderindex { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual Gymworkoutdetail Gymworkout { get; set; } = null!;
}
