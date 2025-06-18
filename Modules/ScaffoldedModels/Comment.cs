using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid? Postid { get; set; }

    public Guid? Userid { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual Post? Post { get; set; }

    public virtual User? User { get; set; }
}
