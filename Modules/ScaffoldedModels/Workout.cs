using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Workout
{
    public Guid Id { get; set; }

    public Guid Userid { get; set; }

    public string Workouttype { get; set; } = null!;

    public DateOnly Date { get; set; }

    public double? Durationminutes { get; set; }

    public string? Notes { get; set; }

    public bool Isprivate { get; set; }

    public bool Isroutine { get; set; }

    public string? Routinename { get; set; }

    public Guid? Createdfromroutineid { get; set; }

    public DateTime? Createdat { get; set; }

    public Guid? Createdby { get; set; }

    public Guid? Updatedby { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual Workout? Createdfromroutine { get; set; }

    public virtual Cyclingworkoutdetail? Cyclingworkoutdetail { get; set; }

    public virtual Gymworkoutdetail? Gymworkoutdetail { get; set; }

    public virtual ICollection<Workout> InverseCreatedfromroutine { get; set; } = new List<Workout>();

    public virtual ICollection<Personalrecordhistory> Personalrecordhistories { get; set; } = new List<Personalrecordhistory>();

    public virtual ICollection<Personalrecord> Personalrecords { get; set; } = new List<Personalrecord>();

    public virtual Runningworkoutdetail? Runningworkoutdetail { get; set; }

    public virtual Swimmingworkoutdetail? Swimmingworkoutdetail { get; set; }

    public virtual User? UpdatedbyNavigation { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Yogaworkoutdetail? Yogaworkoutdetail { get; set; }
}
