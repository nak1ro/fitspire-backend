using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Goal
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public Guid? Goaltypeid { get; set; }

    public double Targetvalue { get; set; }

    public double? Currentvalue { get; set; }

    public string Unit { get; set; } = null!;

    public DateOnly? Deadline { get; set; }

    public virtual Goaltype? Goaltype { get; set; }

    public virtual ICollection<Progressentry> Progressentries { get; set; } = new List<Progressentry>();

    public virtual User? User { get; set; }
}
