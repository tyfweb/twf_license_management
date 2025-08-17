using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.Dashboard;
using TechWayFit.Licensing.Management.Core.Contracts.Services;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardApiController : BaseController
{
    private readonly ILogger<DashboardApiController> _logger;
    private readonly IProductLicenseService _licenseService;
    private readonly IEnterpriseProductService _productService;
    private readonly IConsumerAccountService _consumerService;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public DashboardApiController(
        ILogger<DashboardApiController> logger,
        IProductLicenseService licenseService,
        IEnterpriseProductService productService,
        IConsumerAccountService consumerService,
        IUserService userService,
        IAuditService auditService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _consumerService = consumerService ?? throw new ArgumentNullException(nameof(consumerService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Get dashboard overview with key metrics
    /// </summary>
    /// <returns>Dashboard overview data</returns>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(DashboardOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            _logger.LogInformation("Getting dashboard overview for user {UserId}", CurrentUserName);

            var overview = new DashboardOverviewResponse
            {
                Licenses = await GetLicenseMetrics(),
                Products = await GetProductMetrics(),
                Consumers = await GetConsumerMetrics(),
                Users = await GetUserMetrics(),
                Revenue = await GetRevenueMetrics(),
                RecentActivities = await GetRecentActivities(),
                Alerts = await GetSystemAlerts()
            };

            _logger.LogInformation("Successfully retrieved dashboard overview for user {UserId}", CurrentUserName);
            return Ok(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard overview for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve dashboard overview"));
        }
    }

    /// <summary>
    /// Get analytics data based on request parameters
    /// </summary>
    /// <param name="request">Analytics request parameters</param>
    /// <returns>Analytics data</returns>
    [HttpPost("analytics")]
    [ProducesResponseType(typeof(AnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics([FromBody] GetAnalyticsRequest request)
    {
        try
        {
            _logger.LogInformation("Getting analytics data for user {UserId}, metric type: {MetricType}", 
                CurrentUserName, request.MetricType);

            // Set default date range if not provided
            var endDate = request.EndDate ?? DateTime.UtcNow;
            var startDate = request.StartDate ?? endDate.AddDays(-30);
            var groupBy = request.GroupBy ?? "day";

            var analytics = new AnalyticsResponse
            {
                MetricType = request.MetricType ?? "licenses",
                StartDate = startDate,
                EndDate = endDate,
                GroupBy = groupBy,
                Data = await GetAnalyticsData(request.MetricType ?? "licenses", startDate, endDate, groupBy, request.ProductIds, request.ConsumerIds),
                Summary = await GetAnalyticsSummary(request.MetricType ?? "licenses", startDate, endDate, request.ProductIds, request.ConsumerIds)
            };

            _logger.LogInformation("Successfully retrieved analytics data for user {UserId}", CurrentUserName);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics data for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve analytics data"));
        }
    }

    /// <summary>
    /// Get system status information
    /// </summary>
    /// <returns>System status data</returns>
    [HttpGet("system-status")]
    [ProducesResponseType(typeof(SystemStatusResponse), StatusCodes.Status200OK)]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetSystemStatus()
    {
        try
        {
            _logger.LogInformation("Getting system status for user {UserId}", CurrentUserName);

            var systemStatus = new SystemStatusResponse
            {
                OverallStatus = "Healthy",
                LastChecked = DateTime.UtcNow,
                Services = await GetServiceStatuses(),
                Performance = await GetSystemPerformance(),
                Alerts = await GetSystemStatusAlerts()
            };

            _logger.LogInformation("Successfully retrieved system status for user {UserId}", CurrentUserName);
            return Ok(systemStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system status for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve system status"));
        }
    }

    /// <summary>
    /// Get license metrics for dashboard
    /// </summary>
    /// <returns>License metrics</returns>
    [HttpGet("metrics/licenses")]
    [ProducesResponseType(typeof(LicenseMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLicenseMetricData()
    {
        try
        {
            _logger.LogInformation("Getting license metrics for user {UserId}", CurrentUserName);

            var metrics = await GetLicenseMetrics();

            _logger.LogInformation("Successfully retrieved license metrics for user {UserId}", CurrentUserName);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license metrics for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve license metrics"));
        }
    }

    /// <summary>
    /// Get product metrics for dashboard
    /// </summary>
    /// <returns>Product metrics</returns>
    [HttpGet("metrics/products")]
    [ProducesResponseType(typeof(ProductMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductMetricData()
    {
        try
        {
            _logger.LogInformation("Getting product metrics for user {UserId}", CurrentUserName);

            var metrics = await GetProductMetrics();

            _logger.LogInformation("Successfully retrieved product metrics for user {UserId}", CurrentUserName);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product metrics for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve product metrics"));
        }
    }

    /// <summary>
    /// Get revenue metrics for dashboard
    /// </summary>
    /// <returns>Revenue metrics</returns>
    [HttpGet("metrics/revenue")]
    [ProducesResponseType(typeof(RevenueMetrics), StatusCodes.Status200OK)]
    [Authorize(Roles = "Admin,SuperAdmin,Finance")]
    public async Task<IActionResult> GetRevenueMetricData()
    {
        try
        {
            _logger.LogInformation("Getting revenue metrics for user {UserId}", CurrentUserName);

            var metrics = await GetRevenueMetrics();

            _logger.LogInformation("Successfully retrieved revenue metrics for user {UserId}", CurrentUserName);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue metrics for user {UserId}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve revenue metrics"));
        }
    }

    #region Private Methods

    private async Task<LicenseMetrics> GetLicenseMetrics()
    {
        // In a real implementation, these would be actual service calls
        // This is a simplified example
        return new LicenseMetrics
        {
            TotalLicenses = 1250,
            ActiveLicenses = 1180,
            ExpiredLicenses = 45,
            ExpiringThisMonth = 25,
            ExpiringThisWeek = 8,
            NewThisMonth = 85,
            GrowthRate = 12.5,
            LicensesByStatus = new Dictionary<string, int>
            {
                { "Active", 1180 },
                { "Expired", 45 },
                { "Pending", 25 }
            },
            LicensesTrend = GenerateTrendData(30)
        };
    }

    private async Task<ProductMetrics> GetProductMetrics()
    {
        return new ProductMetrics
        {
            TotalProducts = 48,
            ActiveProducts = 42,
            NewThisMonth = 6,
            GrowthRate = 8.3,
            MostPopularProducts = new List<ProductPopularity>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Enterprise Suite", LicenseCount = 350, Revenue = 125000, GrowthRate = 15.2 },
                new() { ProductId = Guid.NewGuid(), ProductName = "Professional Tools", LicenseCount = 280, Revenue = 84000, GrowthRate = 10.5 }
            },
            ProductsByStatus = new Dictionary<string, int>
            {
                { "Active", 42 },
                { "Inactive", 6 }
            }
        };
    }

    private async Task<ConsumerMetrics> GetConsumerMetrics()
    {
        return new ConsumerMetrics
        {
            TotalConsumers = 890,
            ActiveConsumers = 825,
            NewThisMonth = 65,
            GrowthRate = 9.2,
            ConsumersByStatus = new Dictionary<string, int>
            {
                { "Active", 825 },
                { "Inactive", 65 }
            },
            ConsumersByRegion = new Dictionary<string, int>
            {
                { "North America", 450 },
                { "Europe", 280 },
                { "Asia Pacific", 160 }
            }
        };
    }

    private async Task<UserMetrics> GetUserMetrics()
    {
        return new UserMetrics
        {
            TotalUsers = 125,
            ActiveUsers = 118,
            OnlineUsers = 23,
            NewThisMonth = 8,
            LastLoginAverage = DateTime.UtcNow.AddDays(-2),
            TopActiveUsers = new List<UserActivity>
            {
                new() { UserId = Guid.NewGuid(), UserName = "john.doe@example.com", ActivityCount = 45, LastActivity = DateTime.UtcNow.AddMinutes(-15) },
                new() { UserId = Guid.NewGuid(), UserName = "jane.smith@example.com", ActivityCount = 38, LastActivity = DateTime.UtcNow.AddMinutes(-30) }
            }
        };
    }

    private async Task<RevenueMetrics> GetRevenueMetrics()
    {
        return new RevenueMetrics
        {
            TotalRevenue = 2850000,
            MonthlyRevenue = 285000,
            ProjectedRevenue = 3200000,
            GrowthRate = 15.8,
            RevenueTrend = GenerateRevenueTrendData(12),
            RevenueByProduct = new Dictionary<string, decimal>
            {
                { "Enterprise Suite", 1425000 },
                { "Professional Tools", 855000 },
                { "Basic License", 570000 }
            }
        };
    }

    private async Task<List<RecentActivity>> GetRecentActivities()
    {
        return new List<RecentActivity>
        {
            new() { Id = Guid.NewGuid(), Type = "License", Description = "New license activated", User = "john.doe@example.com", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
            new() { Id = Guid.NewGuid(), Type = "Product", Description = "Product updated", User = "admin@example.com", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
            new() { Id = Guid.NewGuid(), Type = "Consumer", Description = "New consumer registered", User = "jane.smith@example.com", Timestamp = DateTime.UtcNow.AddMinutes(-30) }
        };
    }

    private async Task<List<Alert>> GetSystemAlerts()
    {
        return new List<Alert>
        {
            new() { Id = Guid.NewGuid(), Type = "Warning", Title = "Licenses Expiring Soon", Message = "8 licenses will expire within the next week", Severity = "Medium", CreatedDate = DateTime.UtcNow.AddHours(-2) },
            new() { Id = Guid.NewGuid(), Type = "Info", Title = "System Update Available", Message = "New version 2.1.0 is available", Severity = "Low", CreatedDate = DateTime.UtcNow.AddHours(-6) }
        };
    }

    private async Task<List<AnalyticsDataPoint>> GetAnalyticsData(string metricType, DateTime startDate, DateTime endDate, string groupBy, List<Guid>? productIds, List<Guid>? consumerIds)
    {
        // This would contain actual analytics logic
        var data = new List<AnalyticsDataPoint>();
        var current = startDate;

        while (current <= endDate)
        {
            data.Add(new AnalyticsDataPoint
            {
                Date = current,
                Period = current.ToString(groupBy == "month" ? "yyyy-MM" : "yyyy-MM-dd"),
                Value = Random.Shared.Next(50, 200),
                Count = Random.Shared.Next(10, 50)
            });

            current = groupBy switch
            {
                "month" => current.AddMonths(1),
                "week" => current.AddDays(7),
                _ => current.AddDays(1)
            };
        }

        return data;
    }

    private async Task<AnalyticsSummary> GetAnalyticsSummary(string metricType, DateTime startDate, DateTime endDate, List<Guid>? productIds, List<Guid>? consumerIds)
    {
        return new AnalyticsSummary
        {
            Total = 5420,
            Average = 175.2m,
            Maximum = 245,
            Minimum = 95,
            GrowthRate = 12.5,
            Trend = "up"
        };
    }

    private async Task<List<ServiceStatus>> GetServiceStatuses()
    {
        return new List<ServiceStatus>
        {
            new() { Name = "Database", Status = "Healthy", ResponseTime = TimeSpan.FromMilliseconds(45), LastChecked = DateTime.UtcNow },
            new() { Name = "Cache", Status = "Healthy", ResponseTime = TimeSpan.FromMilliseconds(12), LastChecked = DateTime.UtcNow },
            new() { Name = "Email Service", Status = "Warning", ResponseTime = TimeSpan.FromMilliseconds(250), LastChecked = DateTime.UtcNow }
        };
    }

    private async Task<SystemPerformance> GetSystemPerformance()
    {
        return new SystemPerformance
        {
            CpuUsage = 45.2,
            MemoryUsage = 68.5,
            DiskUsage = 72.1,
            ActiveConnections = 156,
            RequestsPerSecond = 85,
            AverageResponseTime = TimeSpan.FromMilliseconds(125)
        };
    }

    private async Task<List<SystemAlert>> GetSystemStatusAlerts()
    {
        return new List<SystemAlert>
        {
            new() { Type = "Performance", Message = "High memory usage detected", Severity = "Warning", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
            new() { Type = "Service", Message = "Email service response time elevated", Severity = "Info", Timestamp = DateTime.UtcNow.AddMinutes(-30) }
        };
    }

    private List<ChartDataPoint> GenerateTrendData(int days)
    {
        var data = new List<ChartDataPoint>();
        for (int i = days; i >= 0; i--)
        {
            data.Add(new ChartDataPoint
            {
                Date = DateTime.UtcNow.AddDays(-i),
                Value = Random.Shared.Next(80, 150),
                Label = DateTime.UtcNow.AddDays(-i).ToString("MMM dd")
            });
        }
        return data;
    }

    private List<ChartDataPoint> GenerateRevenueTrendData(int months)
    {
        var data = new List<ChartDataPoint>();
        for (int i = months; i >= 0; i--)
        {
            data.Add(new ChartDataPoint
            {
                Date = DateTime.UtcNow.AddMonths(-i),
                Value = Random.Shared.Next(200000, 350000),
                Label = DateTime.UtcNow.AddMonths(-i).ToString("MMM yyyy")
            });
        }
        return data;
    }

    #endregion
}
