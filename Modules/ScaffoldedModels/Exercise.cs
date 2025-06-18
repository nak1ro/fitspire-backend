using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Exercise
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? Categoryid { get; set; }

    public virtual Exercisecategory? Category { get; set; }

    public virtual ICollection<Gymworkoutexercise> Gymworkoutexercises { get; set; } = new List<Gymworkoutexercise>();
}
