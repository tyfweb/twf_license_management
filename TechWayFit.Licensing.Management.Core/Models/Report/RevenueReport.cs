namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Revenue report
/// </summary>
public class RevenueReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public ReportGroupBy GroupBy { get; set; }
    public Dictionary<DateTime, decimal> RevenueByPeriod { get; set; } = new();
    public Dictionary<string, decimal> RevenueByProduct { get; set; } = new();
    public Dictionary<string, decimal> RevenueByTier { get; set; } = new();
}
