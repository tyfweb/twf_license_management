namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Compliance audit report
/// </summary>
public class ComplianceAuditReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public ComplianceType ComplianceType { get; set; }
    public double ComplianceScore { get; set; }
    public IEnumerable<ComplianceIssue> ComplianceIssues { get; set; } = new List<ComplianceIssue>();
    public Dictionary<string, bool> ComplianceChecks { get; set; } = new();
}

/// <summary>
/// Compliance issue details
/// </summary>
public class ComplianceIssue
{
    public string IssueId { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public string? Resolution { get; set; }
    public bool IsResolved { get; set; }
}
