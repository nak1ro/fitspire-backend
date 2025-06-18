using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Progressentry
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public Guid? Goalid { get; set; }

    public DateOnly Date { get; set; }

    public double? Weight { get; set; }

    public double? Bodyfat { get; set; }

    public string? Notes { get; set; }

    public virtual Goal? Goal { get; set; }

    public virtual User? User { get; set; }
}
