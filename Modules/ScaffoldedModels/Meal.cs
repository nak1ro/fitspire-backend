using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Meal
{
    public Guid Id { get; set; }

    public Guid? Userid { get; set; }

    public DateOnly Date { get; set; }

    public double? Totalcalories { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Mealitem> Mealitems { get; set; } = new List<Mealitem>();

    public virtual User? User { get; set; }
}
