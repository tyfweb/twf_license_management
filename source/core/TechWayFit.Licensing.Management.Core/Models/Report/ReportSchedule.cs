namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Report schedule configuration
/// </summary>
public class ReportSchedule
{
    public string ScheduleId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string CronExpression { get; set; } = string.Empty;
    public IEnumerable<string> Recipients { get; set; } = new List<string>();
    public ExportFormat ExportFormat { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastExecuted { get; set; }
    public DateTime? NextExecution { get; set; }
}

/// <summary>
/// Report execution details
/// </summary>
public class ReportExecution
{
    public string ExecutionId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? ScheduleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int RecordsProcessed { get; set; }
    public string ExecutedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
