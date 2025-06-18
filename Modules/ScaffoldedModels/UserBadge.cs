using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class UserBadge
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public Guid? Badgeid { get; set; }

    public DateTime? Awardedat { get; set; }

    public virtual Badge? Badge { get; set; }

    public virtual User? User { get; set; }
}
