using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Core.Models.License;

/// <summary>
/// License usage statistics
/// </summary>
public class LicenseUsageStatistics
{
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int RevokedLicenses { get; set; }
    public int SuspendedLicenses { get; set; }
    public int ExpiringInNext30Days { get; set; }
    public Dictionary<LicenseStatus, int> LicensesByStatus { get; set; } = new();
    public Dictionary<string, int> LicensesByProduct { get; set; } = new();
}
