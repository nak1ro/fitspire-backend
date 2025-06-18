using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Post
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public string Content { get; set; } = null!;

    public string? Imageurl { get; set; }

    public DateTime? Createdat { get; set; }

    public Guid? Createdby { get; set; }

    public Guid? Updatedby { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Savedpost> Savedposts { get; set; } = new List<Savedpost>();

    public virtual User? UpdatedbyNavigation { get; set; }

    public virtual User? User { get; set; }
}
