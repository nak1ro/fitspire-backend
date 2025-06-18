using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Follower
{
    public Guid Id { get; set; }

    public Guid? Followerid { get; set; }

    public Guid? Followedid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? Followed { get; set; }

    public virtual User? FollowerNavigation { get; set; }
}
