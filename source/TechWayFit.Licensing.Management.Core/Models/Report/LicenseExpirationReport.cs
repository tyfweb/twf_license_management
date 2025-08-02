using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// License expiration report
/// </summary>
public class LicenseExpirationReport
{
    public DateTime ReportDate { get; set; }
    public int DaysAhead { get; set; }
    public IEnumerable<ExpiringLicense> ExpiringLicenses { get; set; } = new List<ExpiringLicense>();
    public Dictionary<int, int> ExpirationsByDays { get; set; } = new();
    public Dictionary<string, int> ExpirationsByProduct { get; set; } = new();
}

/// <summary>
/// Expiring license details
/// </summary>
public class ExpiringLicense
{
    public string LicenseId { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public LicenseStatus Status { get; set; }
}
