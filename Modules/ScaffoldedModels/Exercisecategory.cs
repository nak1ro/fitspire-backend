using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Exercisecategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
