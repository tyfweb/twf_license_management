namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Product performance report
/// </summary>
public class ProductPerformanceReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IEnumerable<ProductPerformance> ProductPerformances { get; set; } = new List<ProductPerformance>();
}

/// <summary>
/// Product performance details
/// </summary>
public class ProductPerformance
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int NewLicenses { get; set; }
    public int RenewedLicenses { get; set; }
    public decimal Revenue { get; set; }
    public double CustomerSatisfaction { get; set; }
}
