using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// License usage report
/// </summary>
public class LicenseUsageReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalActiveLicenses { get; set; }
    public int TotalInactiveLicenses { get; set; }
    public int NewLicensesIssued { get; set; }
    public int LicensesExpired { get; set; }
    public int LicensesRevoked { get; set; }
    public Dictionary<string, int> UsageByProduct { get; set; } = new();
    public Dictionary<string, int> UsageByConsumer { get; set; } = new();
    public Dictionary<LicenseStatus, int> UsageByStatus { get; set; } = new();
}
