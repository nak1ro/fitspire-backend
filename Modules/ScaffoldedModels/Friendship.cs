using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Friendship
{
    public Guid Id { get; set; }

    public Guid User1id { get; set; }

    public Guid User2id { get; set; }

    public Guid? Requestid { get; set; }

    public DateTime? Becamefriendsat { get; set; }

    public virtual Friendshiprequest? Request { get; set; }

    public virtual User User1 { get; set; } = null!;

    public virtual User User2 { get; set; } = null!;
}
