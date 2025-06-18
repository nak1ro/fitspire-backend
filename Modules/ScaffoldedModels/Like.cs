using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Like
{
    public Guid Id { get; set; }

    public Guid Userid { get; set; }

    public Guid Targetid { get; set; }

    public string Targettype { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual User User { get; set; } = null!;
}
