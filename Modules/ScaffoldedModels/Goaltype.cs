using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Goaltype
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Unit { get; set; }

    public string? Category { get; set; }

    public string? Iconurl { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
