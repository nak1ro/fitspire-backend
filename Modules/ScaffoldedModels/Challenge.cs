using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Challenge
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public Guid? Createdby { get; set; }

    public virtual ICollection<Challengeparticipant> Challengeparticipants { get; set; } = new List<Challengeparticipant>();

    public virtual User? CreatedbyNavigation { get; set; }
}
