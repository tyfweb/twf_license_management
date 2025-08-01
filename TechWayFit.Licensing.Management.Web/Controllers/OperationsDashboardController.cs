using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Web.ViewModels.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.OperationsDashboard;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for Operations Dashboard functionality
/// </summary>
// [Authorize(Roles = "Administrator")] // Temporarily disabled for testing
[Route("OperationsDashboard")]
public class OperationsDashboardController : Controller
{
    private readonly IOperationsDashboardService _dashboardService;
    private readonly ILogger<OperationsDashboardController> _logger;

    public OperationsDashboardController(
        IOperationsDashboardService dashboardService,
        ILogger<OperationsDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    #region Helper Methods

    /// <summary>
    /// Get start and end dates based on hours from now
    /// </summary>
    private (DateTime startTime, DateTime endTime) GetTimeRange(int hours)
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-hours);
        return (startTime, endTime);
    }

    /// <summary>
    /// Cast object result to specific type safely
    /// </summary>
    private T? CastResult<T>(object? result) where T : class
    {
        if (result == null) return null;
        return result as T;
    }

    /// <summary>
    /// Cast object result to enumerable safely
    /// </summary>
    private IEnumerable<T> CastToEnumerable<T>(object? result) where T : class
    {
        if (result == null) return new List<T>();
        return result as IEnumerable<T> ?? new List<T>();
    }

    /// <summary>
    /// Get default dashboard configuration
    /// </summary>
    private DashboardConfiguration GetDefaultConfiguration()
    {
        return new DashboardConfiguration
        {
            IsEnabled = true,
            RefreshIntervalSeconds = 30,
            MetricsCollectionEnabled = true,
            PerformanceMonitoringEnabled = true,
            ErrorTrackingEnabled = true
        };
    }

    #endregion

    /// <summary>
    /// Main dashboard view
    /// </summary>
    /// <returns>Dashboard view</returns>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(24); // Last 24 hours

            var viewModel = new OperationsDashboardViewModel
            {
                CurrentHealth = CastResult<SystemHealthSnapshotEntity>(await _dashboardService.GetCurrentSystemHealthAsync()),
                SystemMetricsOverview = CastToEnumerable<SystemMetricEntity>(await _dashboardService.GetSystemMetricsAsync(startTime, endTime)),
                ErrorSummary = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetErrorSummaryAsync(startTime, endTime)),
                PerformanceMetrics = CastToEnumerable<PagePerformanceMetricEntity>(await _dashboardService.GetPagePerformanceAsync(startTime, endTime)),
                SlowestEndpoints = CastToEnumerable<PagePerformanceMetricEntity>(await _dashboardService.GetSlowestEndpointsAsync(topCount: 5)),
                TopErrors = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetTopErrorsAsync(topCount: 5)),
                Configuration = GetDefaultConfiguration()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading operations dashboard");
            
            // Return the view with empty data instead of an error view
            var emptyViewModel = new OperationsDashboardViewModel
            {
                CurrentHealth = null,
                SystemMetricsOverview = new List<SystemMetricEntity>(),
                ErrorSummary = new List<ErrorLogSummaryEntity>(),
                PerformanceMetrics = new List<PagePerformanceMetricEntity>(),
                SlowestEndpoints = new List<PagePerformanceMetricEntity>(),
                TopErrors = new List<ErrorLogSummaryEntity>(),
                Configuration = GetDefaultConfiguration()
            };
            
            // Add error message to ViewBag for display
            ViewBag.ErrorMessage = "Unable to load dashboard data. Please check the database connection and try again.";
            return View(emptyViewModel);
        }
    }

    /// <summary>
    /// Real-time dashboard view with auto-refresh
    /// </summary>
    /// <returns>Real-time dashboard view</returns>
    [HttpGet("RealTime")]
    public async Task<IActionResult> RealTime()
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(1); // Last hour

            var viewModel = new RealTimeDashboardViewModel
            {
                CurrentHealth = CastResult<SystemHealthSnapshotEntity>(await _dashboardService.GetCurrentSystemHealthAsync()),
                SystemMetricsOverview = CastToEnumerable<SystemMetricEntity>(await _dashboardService.GetSystemMetricsAsync(startTime, endTime)),
                RecentErrors = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetTopErrorsAsync(10, startTime, endTime)),
                Configuration = GetDefaultConfiguration()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading real-time dashboard");
            return View("Error");
        }
    }

    /// <summary>
    /// Performance analysis view
    /// </summary>
    /// <param name="hours">Number of hours to analyze (default: 24)</param>
    /// <returns>Performance analysis view</returns>
    [HttpGet("Performance")]
    public async Task<IActionResult> Performance(int hours = 24)
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(hours);

            var viewModel = new PerformanceAnalysisViewModel
            {
                TimeRange = hours,
                PerformanceMetrics = CastToEnumerable<PagePerformanceMetricEntity>(await _dashboardService.GetPagePerformanceAsync(startTime, endTime)),
                SlowestEndpoints = CastToEnumerable<PagePerformanceMetricEntity>(await _dashboardService.GetSlowestEndpointsAsync(20, startTime, endTime)),
                SlowestQueries = CastToEnumerable<QueryPerformanceMetricEntity>(await _dashboardService.GetSlowestQueriesAsync(20, startTime, endTime)),
                PerformanceTrends = CastToEnumerable<PerformanceTrend>(await _dashboardService.GetSystemPerformanceTrendsAsync(Math.Max(1, hours / 24))),
                Configuration = GetDefaultConfiguration()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading performance analysis");
            return View("Error");
        }
    }

    /// <summary>
    /// Error analysis view
    /// </summary>
    /// <param name="hours">Number of hours to analyze (default: 24)</param>
    /// <returns>Error analysis view</returns>
    [HttpGet("Errors")]
    public async Task<IActionResult> Errors(int hours = 24)
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(hours);

            var viewModel = new ErrorAnalysisViewModel
            {
                TimeRange = hours,
                ErrorSummary = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetErrorSummaryAsync(startTime, endTime)),
                TopErrors = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetTopErrorsAsync(50, startTime, endTime)),
                ErrorTrends = new List<ErrorTrend>(), // Service doesn't have this method, using empty list
                Configuration = GetDefaultConfiguration()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading error analysis");
            return View("Error");
        }
    }

    /// <summary>
    /// System health monitoring view
    /// </summary>
    /// <param name="hours">Number of hours to analyze (default: 24)</param>
    /// <returns>System health view</returns>
    [HttpGet("Health")]
    public async Task<IActionResult> Health(int hours = 24)
    {
        try
        {
            var viewModel = new SystemHealthViewModel
            {
                TimeRange = hours,
                CurrentHealth = CastResult<SystemHealthSnapshotEntity>(await _dashboardService.GetCurrentSystemHealthAsync()),
                HealthTrends = CastToEnumerable<HealthTrend>(await _dashboardService.GetHealthTrendsAsync(hours)),
                Configuration = GetDefaultConfiguration()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system health");
            return View("Error");
        }
    }

    /// <summary>
    /// Detailed reports view
    /// </summary>
    /// <param name="startDate">Start date for the report</param>
    /// <param name="endDate">End date for the report</param>
    /// <param name="reportType">Type of report (metrics, errors, performance, queries)</param>
    /// <returns>Detailed reports view</returns>
    [HttpGet("Reports")]
    public async Task<IActionResult> Reports(DateTime? startDate = null, DateTime? endDate = null, string reportType = "metrics")
    {
        try
        {
            startDate ??= DateTime.UtcNow.AddDays(-7);
            endDate ??= DateTime.UtcNow;

            var viewModel = new DetailedReportsViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                ReportType = reportType,
                Configuration = GetDefaultConfiguration()
            };

            // Load data based on report type
            switch (reportType.ToLower())
            {
                case "metrics":
                    viewModel.DetailedMetrics = CastToEnumerable<SystemMetricEntity>(await _dashboardService.GetSystemMetricsAsync(startDate.Value, endDate.Value));
                    break;
                case "errors":
                    viewModel.DetailedErrors = CastToEnumerable<ErrorLogSummaryEntity>(await _dashboardService.GetErrorSummaryAsync(startDate.Value, endDate.Value));
                    break;
                case "performance":
                    viewModel.DetailedPerformance = CastToEnumerable<PagePerformanceMetricEntity>(await _dashboardService.GetPagePerformanceAsync(startDate.Value, endDate.Value));
                    break;
                case "queries":
                    viewModel.DetailedQueries = CastToEnumerable<QueryPerformanceMetricEntity>(await _dashboardService.GetQueryPerformanceAsync(startDate.Value, endDate.Value));
                    break;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading detailed reports");
            return View("Error");
        }
    }

    /// <summary>
    /// API endpoint for getting current system health data
    /// </summary>
    /// <returns>JSON with current health data</returns>
    [HttpGet("Api/Health")]
    public async Task<IActionResult> GetHealthData()
    {
        try
        {
            var health = await _dashboardService.GetCurrentSystemHealthAsync();
            return Json(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health data");
            return StatusCode(500, new { error = "Failed to retrieve health data" });
        }
    }

    /// <summary>
    /// API endpoint for getting metrics overview
    /// </summary>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>JSON with metrics overview</returns>
    [HttpGet("Api/Metrics")]
    public async Task<IActionResult> GetMetricsData(int hours = 1)
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(hours);
            var metrics = await _dashboardService.GetSystemMetricsAsync(startTime, endTime);
            return Json(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics data");
            return StatusCode(500, new { error = "Failed to retrieve metrics data" });
        }
    }

    /// <summary>
    /// API endpoint for getting error summary
    /// </summary>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>JSON with error summary</returns>
    [HttpGet("Api/Errors")]
    public async Task<IActionResult> GetErrorsData(int hours = 1)
    {
        try
        {
            var (startTime, endTime) = GetTimeRange(hours);
            var errors = await _dashboardService.GetErrorSummaryAsync(startTime, endTime);
            return Json(errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting errors data");
            return StatusCode(500, new { error = "Failed to retrieve errors data" });
        }
    }

    /// <summary>
    /// API endpoint for getting performance trends
    /// </summary>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>JSON with performance trends</returns>
    [HttpGet("Api/Performance")]
    public async Task<IActionResult> GetPerformanceData(int hours = 24)
    {
        try
        {
            var performance = await _dashboardService.GetSystemPerformanceTrendsAsync(Math.Max(1, hours / 24));
            return Json(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance data");
            return StatusCode(500, new { error = "Failed to retrieve performance data" });
        }
    }

    /// <summary>
    /// Exports dashboard data to CSV
    /// </summary>
    /// <param name="dataType">Type of data to export</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>CSV file download</returns>
    [HttpGet("Export")]
    public async Task<IActionResult> ExportData(string dataType, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Use the available export methods from the service
            object csvData = dataType.ToLower() switch
            {
                "metrics" => await _dashboardService.ExportMetricsAsync("csv", startDate, endDate),
                "errors" => await _dashboardService.ExportErrorReportAsync("csv", startDate, endDate),
                "performance" => await _dashboardService.ExportPerformanceReportAsync("csv", startDate, endDate),
                _ => await _dashboardService.ExportMetricsAsync("csv", startDate, endDate)
            };

            var fileName = $"operations-dashboard-{dataType}-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.csv";
            
            if (csvData is byte[] byteData)
            {
                return File(byteData, "text/csv", fileName);
            }
            else if (csvData is string stringData)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(stringData);
                return File(bytes, "text/csv", fileName);
            }
            else
            {
                return StatusCode(500, new { error = "Invalid export data format" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting dashboard data");
            return StatusCode(500, new { error = "Failed to export data" });
        }
    }

    /// <summary>
    /// Gets buffer status for metrics collection (placeholder - not implemented in service)
    /// </summary>
    /// <returns>JSON with buffer status</returns>
    [HttpGet("Api/BufferStatus")]
    public IActionResult GetBufferStatus()
    {
        try
        {
            // This functionality is not available in the current service interface
            // Return a placeholder response
            var status = new { bufferSize = 0, itemsInBuffer = 0, lastFlush = DateTime.UtcNow };
            return Json(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting buffer status");
            return StatusCode(500, new { error = "Failed to retrieve buffer status" });
        }
    }

    /// <summary>
    /// Manually flushes metrics buffer (placeholder - not implemented in service)
    /// </summary>
    /// <returns>JSON with flush result</returns>
    [HttpPost("Api/FlushMetrics")]
    public IActionResult FlushMetrics()
    {
        try
        {
            // This functionality is not available in the current service interface
            // Return a placeholder response
            var flushedCount = 0;
            return Json(new { success = true, flushedCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing metrics");
            return StatusCode(500, new { error = "Failed to flush metrics" });
        }
    }
}
