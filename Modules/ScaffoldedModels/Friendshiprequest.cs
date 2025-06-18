using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Friendshiprequest
{
    public Guid Id { get; set; }

    public Guid Requesterid { get; set; }

    public Guid Addresseeid { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? Requestedat { get; set; }

    public DateTime? Respondedat { get; set; }

    public virtual User Addressee { get; set; } = null!;

    public virtual Friendship? Friendship { get; set; }

    public virtual User Requester { get; set; } = null!;
}
