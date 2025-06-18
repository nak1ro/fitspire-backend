using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Badge
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
