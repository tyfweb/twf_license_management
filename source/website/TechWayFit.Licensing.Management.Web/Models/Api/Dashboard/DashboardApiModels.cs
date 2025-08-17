namespace TechWayFit.Licensing.Management.Web.Models.Api.Dashboard;

public class DashboardOverviewResponse
{
    public LicenseMetrics Licenses { get; set; } = new();
    public ProductMetrics Products { get; set; } = new();
    public ConsumerMetrics Consumers { get; set; } = new();
    public UserMetrics Users { get; set; } = new();
    public RevenueMetrics Revenue { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<Alert> Alerts { get; set; } = new();
}

public class LicenseMetrics
{
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int ExpiringThisMonth { get; set; }
    public int ExpiringThisWeek { get; set; }
    public int NewThisMonth { get; set; }
    public double GrowthRate { get; set; }
    public Dictionary<string, int> LicensesByStatus { get; set; } = new();
    public List<ChartDataPoint> LicensesTrend { get; set; } = new();
}

public class ProductMetrics
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int NewThisMonth { get; set; }
    public double GrowthRate { get; set; }
    public List<ProductPopularity> MostPopularProducts { get; set; } = new();
    public Dictionary<string, int> ProductsByStatus { get; set; } = new();
}

public class ConsumerMetrics
{
    public int TotalConsumers { get; set; }
    public int ActiveConsumers { get; set; }
    public int NewThisMonth { get; set; }
    public double GrowthRate { get; set; }
    public Dictionary<string, int> ConsumersByStatus { get; set; } = new();
    public Dictionary<string, int> ConsumersByRegion { get; set; } = new();
}

public class UserMetrics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int OnlineUsers { get; set; }
    public int NewThisMonth { get; set; }
    public DateTime LastLoginAverage { get; set; }
    public List<UserActivity> TopActiveUsers { get; set; } = new();
}

public class RevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public double GrowthRate { get; set; }
    public List<ChartDataPoint> RevenueTrend { get; set; } = new();
    public Dictionary<string, decimal> RevenueByProduct { get; set; } = new();
}

public class RecentActivity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
}

public class Alert
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
}

public class ChartDataPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string? Label { get; set; }
}

public class ProductPopularity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int LicenseCount { get; set; }
    public decimal Revenue { get; set; }
    public double GrowthRate { get; set; }
}

public class UserActivity
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> RecentActions { get; set; } = new();
}

public class GetAnalyticsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MetricType { get; set; }
    public string? GroupBy { get; set; } // day, week, month
    public List<Guid>? ProductIds { get; set; }
    public List<Guid>? ConsumerIds { get; set; }
}

public class AnalyticsResponse
{
    public string MetricType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string GroupBy { get; set; } = string.Empty;
    public List<AnalyticsDataPoint> Data { get; set; } = new();
    public AnalyticsSummary Summary { get; set; } = new();
}

public class AnalyticsDataPoint
{
    public DateTime Date { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
    public Dictionary<string, object> Dimensions { get; set; } = new();
}

public class AnalyticsSummary
{
    public decimal Total { get; set; }
    public decimal Average { get; set; }
    public decimal Maximum { get; set; }
    public decimal Minimum { get; set; }
    public double GrowthRate { get; set; }
    public string Trend { get; set; } = string.Empty; // up, down, stable
}

public class SystemStatusResponse
{
    public string OverallStatus { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public List<ServiceStatus> Services { get; set; } = new();
    public SystemPerformance Performance { get; set; } = new();
    public List<SystemAlert> Alerts { get; set; } = new();
}

public class ServiceStatus
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastChecked { get; set; }
    public string? Version { get; set; }
}

public class SystemPerformance
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveConnections { get; set; }
    public int RequestsPerSecond { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
}

public class SystemAlert
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
