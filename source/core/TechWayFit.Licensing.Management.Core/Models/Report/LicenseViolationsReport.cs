namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// License violations report
/// </summary>
public class LicenseViolationsReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalViolations { get; set; }
    public IEnumerable<LicenseViolation> Violations { get; set; } = new List<LicenseViolation>();
    public Dictionary<ViolationType, int> ViolationsByType { get; set; } = new();
}

/// <summary>
/// License violation details
/// </summary>
public class LicenseViolation
{
    public string ViolationId { get; set; } = string.Empty;
    public ViolationType ViolationType { get; set; }
    public string LicenseId { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
}
