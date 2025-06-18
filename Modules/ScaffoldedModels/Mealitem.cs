using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Mealitem
{
    public Guid Id { get; set; }

    public Guid? Mealid { get; set; }

    public string Name { get; set; } = null!;

    public double? Calories { get; set; }

    public double? Protein { get; set; }

    public double? Carbs { get; set; }

    public double? Fat { get; set; }

    public double? Quantity { get; set; }

    public string? Unit { get; set; }

    public virtual Meal? Meal { get; set; }
}
