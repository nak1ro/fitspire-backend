using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Groupmember
{
    public Guid Id { get; set; }

    public Guid? Groupid { get; set; }

    public Guid? Userid { get; set; }

    public DateTime? Joinedat { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? User { get; set; }
}
