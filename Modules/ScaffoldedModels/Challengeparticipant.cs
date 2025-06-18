using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Challengeparticipant
{
    public Guid Id { get; set; }

    public Guid? Challengeid { get; set; }

    public Guid? Userid { get; set; }

    public double? Score { get; set; }

    public virtual Challenge? Challenge { get; set; }

    public virtual User? User { get; set; }
}
