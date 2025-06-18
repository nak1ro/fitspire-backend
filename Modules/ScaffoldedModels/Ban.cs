using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Ban
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public string? Reason { get; set; }

    public DateTime? Bannedat { get; set; }

    public DateTime? Expiresat { get; set; }

    public virtual User? User { get; set; }
}
