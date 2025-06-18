using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Group
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Isprivate { get; set; }

    public Guid? Createdby { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Groupmember> Groupmembers { get; set; } = new List<Groupmember>();
}
