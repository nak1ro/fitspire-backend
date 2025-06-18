using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Userpreference
{
    public Guid Id { get; set; }

    public Guid Userid { get; set; }

    public string? Preferredlanguage { get; set; }

    public bool? Isdarkmodeenabled { get; set; }

    public bool? Receiveemailnotifications { get; set; }

    public string? Unitsystem { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual User User { get; set; } = null!;
}
