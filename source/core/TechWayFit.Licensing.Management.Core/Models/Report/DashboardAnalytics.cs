namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Dashboard analytics
/// </summary>
public class DashboardAnalytics
{
    public DateTime ReportDate { get; set; }
    public DateRange DateRange { get; set; }
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiringLicenses { get; set; }
    public int NewLicenses { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalConsumers { get; set; }
    public int ActiveProducts { get; set; }
    public Dictionary<string, int> TopProducts { get; set; } = new();
    public Dictionary<DateTime, int> LicensesTrend { get; set; } = new();
    public Dictionary<DateTime, decimal> RevenueTrend { get; set; } = new();
}
