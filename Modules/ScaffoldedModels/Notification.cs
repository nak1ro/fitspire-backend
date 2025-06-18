using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public string Message { get; set; } = null!;

    public bool? Isread { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? User { get; set; }
}
