using System;
using System.Collections.Generic;

namespace backend.Modules.ScaffoldedModels;

public partial class Report
{
    public Guid Id { get; set; }

    public Guid? Reportedby { get; set; }

    public Guid? Reporteduser { get; set; }

    public string? Reason { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? ReportedbyNavigation { get; set; }

    public virtual User? ReporteduserNavigation { get; set; }
}
