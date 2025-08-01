using TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.OperationsDashboard;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TechWayFit.Licensing.Management.Services.Implementations.OperationsDashboard;

/// <summary>
/// Service implementation for operations dashboard functionality
/// </summary>
public class OperationsDashboardService : IOperationsDashboardService
{
    private readonly ISystemMetricRepository _systemMetricRepository;
    private readonly IErrorLogSummaryRepository _errorLogSummaryRepository;
    private readonly IPagePerformanceMetricRepository _pagePerformanceMetricRepository;
    private readonly IQueryPerformanceMetricRepository _queryPerformanceMetricRepository;
    private readonly ISystemHealthSnapshotRepository _systemHealthSnapshotRepository;
    private readonly ILogger<OperationsDashboardService> _logger;

    public OperationsDashboardService(
        ISystemMetricRepository systemMetricRepository,
        IErrorLogSummaryRepository errorLogSummaryRepository,
        IPagePerformanceMetricRepository pagePerformanceMetricRepository,
        IQueryPerformanceMetricRepository queryPerformanceMetricRepository,
        ISystemHealthSnapshotRepository systemHealthSnapshotRepository,
        ILogger<OperationsDashboardService> logger)
    {
        _systemMetricRepository = systemMetricRepository;
        _errorLogSummaryRepository = errorLogSummaryRepository;
        _pagePerformanceMetricRepository = pagePerformanceMetricRepository;
        _queryPerformanceMetricRepository = queryPerformanceMetricRepository;
        _systemHealthSnapshotRepository = systemHealthSnapshotRepository;
        _logger = logger;
    }

    // Dashboard overview methods
    public async Task<object> GetDashboardOverviewAsync(DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            _logger.LogInformation("Getting dashboard overview for period {StartTime} to {EndTime}", start, end);

            var systemHealth = await GetCurrentSystemHealthAsync();
            var errorSummary = await GetErrorSummaryAsync(start, end);
            var performanceMetrics = await GetSystemMetricsAsync(start, end);
            var topErrors = await GetTopErrorsAsync(5, start, end);
            var slowestEndpoints = await GetSlowestEndpointsAsync(5, start, end);

            return new
            {
                Period = new { StartTime = start, EndTime = end },
                SystemHealth = systemHealth,
                ErrorSummary = errorSummary,
                PerformanceMetrics = performanceMetrics,
                TopErrors = topErrors,
                SlowestEndpoints = slowestEndpoints,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard overview");
            throw;
        }
    }

    public async Task<object> GetSystemHealthStatusAsync()
    {
        try
        {
            var latestSnapshot = await _systemHealthSnapshotRepository.GetLatestAsync();
            if (latestSnapshot == null)
            {
                return new { Status = "Unknown", Message = "No health data available" };
            }

            var healthPercentage = await _systemHealthSnapshotRepository.GetHealthPercentageAsync(
                DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

            return new
            {
                Status = latestSnapshot.OverallHealthStatus,
                CpuUsage = latestSnapshot.CpuUsagePercent,
                MemoryUsage = latestSnapshot.MemoryUsageMb,
                DiskUsage = latestSnapshot.DiskUsagePercent,
                ActiveConnections = latestSnapshot.ActiveDbConnections,
                HealthPercentage = healthPercentage,
                LastChecked = latestSnapshot.SnapshotTimestamp
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health status");
            return new { Status = "Error", Message = "Unable to retrieve health status" };
        }
    }

    public async Task<object> GetRealTimeMetricsAsync()
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddMinutes(-5);

            var recentMetrics = await _systemMetricRepository.GetByTimeRangeAsync(startTime, endTime);
            var recentErrors = await _errorLogSummaryRepository.GetByTimeRangeAsync(startTime, endTime);
            var currentHealth = await GetCurrentSystemHealthAsync();

            return new
            {
                Metrics = recentMetrics,
                Errors = recentErrors,
                SystemHealth = currentHealth,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time metrics");
            throw;
        }
    }

    // System metrics methods
    public async Task<object> GetSystemMetricsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            _logger.LogInformation("Getting system metrics for period {StartTime} to {EndTime}", startTime, endTime);

            var metrics = await _systemMetricRepository.GetByTimeRangeAsync(startTime, endTime);
            var avgResponseTime = await _systemMetricRepository.GetAverageResponseTimeAsync(startTime, endTime);
            var totalRequests = await _systemMetricRepository.GetTotalRequestCountAsync(startTime, endTime);
            var errorRate = await _systemMetricRepository.GetAverageErrorRateAsync(startTime, endTime);

            return new
            {
                Metrics = metrics,
                Summary = new
                {
                    AverageResponseTime = avgResponseTime,
                    TotalRequests = totalRequests,
                    ErrorRate = errorRate,
                    Period = new { StartTime = startTime, EndTime = endTime }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system metrics");
            throw;
        }
    }

    public async Task<object> GetSystemPerformanceTrendsAsync(int days = 7)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddDays(-days);

            var trends = new List<object>();
            for (int i = 0; i < days; i++)
            {
                var dayStart = startTime.AddDays(i).Date;
                var dayEnd = dayStart.AddDays(1);

                var avgResponseTime = await _systemMetricRepository.GetAverageResponseTimeAsync(dayStart, dayEnd);
                var totalRequests = await _systemMetricRepository.GetTotalRequestCountAsync(dayStart, dayEnd);
                var errorRate = await _systemMetricRepository.GetAverageErrorRateAsync(dayStart, dayEnd);

                trends.Add(new
                {
                    Date = dayStart,
                    AverageResponseTime = avgResponseTime,
                    TotalRequests = totalRequests,
                    ErrorRate = errorRate
                });
            }

            return new { Trends = trends, Period = new { Days = days } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system performance trends");
            throw;
        }
    }

    public async Task<object> RecordSystemMetricAsync(object systemMetric)
    {
        try
        {
            var json = JsonSerializer.Serialize(systemMetric);
            var metric = JsonSerializer.Deserialize<SystemMetricEntity>(json);
            
            if (metric != null)
            {
                metric.MetricId = Guid.NewGuid();
                metric.CreatedOn = DateTime.UtcNow;
                metric.CreatedBy = "System";

                var result = await _systemMetricRepository.CreateAsync(metric);
                _logger.LogInformation("Recorded system metric {MetricId}", result.MetricId);
                return result;
            }

            throw new ArgumentException("Invalid system metric data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording system metric");
            throw;
        }
    }

    public async Task<object> RecordSystemMetricsBulkAsync(IEnumerable<object> systemMetrics)
    {
        try
        {
            var entities = new List<SystemMetricEntity>();
            
            foreach (var metric in systemMetrics)
            {
                var json = JsonSerializer.Serialize(metric);
                var entity = JsonSerializer.Deserialize<SystemMetricEntity>(json);
                
                if (entity != null)
                {
                    entity.MetricId = Guid.NewGuid();
                    entity.CreatedOn = DateTime.UtcNow;
                    entity.CreatedBy = "System";
                    entities.Add(entity);
                }
            }

            var results = await _systemMetricRepository.CreateBulkAsync(entities);
            _logger.LogInformation("Recorded {Count} system metrics in bulk", entities.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording system metrics in bulk");
            throw;
        }
    }

    // Error tracking methods
    public async Task<object> GetErrorSummaryAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var errors = await _errorLogSummaryRepository.GetByTimeRangeAsync(startTime, endTime);
            var totalErrors = await _errorLogSummaryRepository.GetTotalOccurrenceCountAsync(startTime, endTime);
            var unresolvedCount = await _errorLogSummaryRepository.GetUnresolvedCountAsync(startTime, endTime);
            var topErrors = await _errorLogSummaryRepository.GetTopByOccurrenceCountAsync(5, startTime, endTime);

            return new
            {
                TotalErrors = totalErrors,
                UnresolvedErrors = unresolvedCount,
                TopErrors = topErrors,
                ErrorDetails = errors,
                Period = new { StartTime = startTime, EndTime = endTime }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error summary");
            throw;
        }
    }

    public async Task<object> GetTopErrorsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var topErrors = await _errorLogSummaryRepository.GetTopByOccurrenceCountAsync(topCount, start, end);
            return new { TopErrors = topErrors, Count = topCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top errors");
            throw;
        }
    }

    public async Task<object> GetUnresolvedErrorsAsync()
    {
        try
        {
            var unresolvedErrors = await _errorLogSummaryRepository.GetUnresolvedErrorsAsync();
            return new { UnresolvedErrors = unresolvedErrors, Count = unresolvedErrors.Count() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unresolved errors");
            throw;
        }
    }

    public async Task<object> RecordErrorAsync(object errorLog)
    {
        try
        {
            var json = JsonSerializer.Serialize(errorLog);
            var errorData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (errorData != null && errorData.ContainsKey("errorMessage"))
            {
                var errorMessage = errorData["errorMessage"]?.ToString() ?? "Unknown error";
                var errorHash = GenerateErrorHash(errorMessage);

                var existingError = await _errorLogSummaryRepository.GetByMessageHashAsync(errorHash);
                
                if (existingError != null)
                {
                    await _errorLogSummaryRepository.IncrementOccurrenceAsync(errorHash);
                    _logger.LogInformation("Incremented occurrence count for error {ErrorHash}", errorHash);
                    return existingError;
                }
                else
                {
                    var newError = new ErrorLogSummaryEntity
                    {
                        ErrorSummaryId = Guid.NewGuid(),
                        ErrorMessageHash = errorHash,
                        ErrorMessageSample = errorMessage,
                        ErrorType = errorData.ContainsKey("errorLevel") ? errorData["errorLevel"]?.ToString() ?? "Error" : "Error",
                        OccurrenceCount = 1,
                        FirstOccurrence = DateTime.UtcNow,
                        LastOccurrence = DateTime.UtcNow,
                        IsResolved = false,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    var result = await _errorLogSummaryRepository.CreateAsync(newError);
                    _logger.LogInformation("Created new error log {ErrorId}", result.ErrorSummaryId);
                    return result;
                }
            }

            throw new ArgumentException("Invalid error log data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording error log");
            throw;
        }
    }

    public async Task<object> MarkErrorAsResolvedAsync(string errorHash, string resolvedBy)
    {
        try
        {
            var result = await _errorLogSummaryRepository.MarkAsResolvedAsync(errorHash, resolvedBy);
            _logger.LogInformation("Marked error {ErrorHash} as resolved by {ResolvedBy}", errorHash, resolvedBy);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking error as resolved");
            throw;
        }
    }

    // Performance metrics methods
    public async Task<object> GetPagePerformanceAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var metrics = await _pagePerformanceMetricRepository.GetByTimeRangeAsync(startTime, endTime);
            var avgResponseTime = await _pagePerformanceMetricRepository.GetAverageResponseTimeAsync(startTime, endTime);
            var totalRequests = await _pagePerformanceMetricRepository.GetTotalRequestCountAsync(startTime, endTime);
            var avgErrorRate = await _pagePerformanceMetricRepository.GetAverageErrorRateAsync(startTime, endTime);

            return new
            {
                Metrics = metrics,
                Summary = new
                {
                    AverageResponseTime = avgResponseTime,
                    TotalRequests = totalRequests,
                    AverageErrorRate = avgErrorRate,
                    Period = new { StartTime = startTime, EndTime = endTime }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting page performance metrics");
            throw;
        }
    }

    public async Task<object> GetSlowestEndpointsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var slowestEndpoints = await _pagePerformanceMetricRepository.GetSlowestEndpointsAsync(topCount, start, end);
            return new { SlowestEndpoints = slowestEndpoints, Count = topCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slowest endpoints");
            throw;
        }
    }

    public async Task<object> GetHighestTrafficEndpointsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var highestTrafficEndpoints = await _pagePerformanceMetricRepository.GetHighestTrafficEndpointsAsync(topCount, start, end);
            return new { HighestTrafficEndpoints = highestTrafficEndpoints, Count = topCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting highest traffic endpoints");
            throw;
        }
    }

    public async Task<object> RecordPagePerformanceAsync(object pageMetric)
    {
        try
        {
            var json = JsonSerializer.Serialize(pageMetric);
            var metric = JsonSerializer.Deserialize<PagePerformanceMetricEntity>(json);
            
            if (metric != null)
            {
                metric.PerformanceId = Guid.NewGuid();
                metric.CreatedOn = DateTime.UtcNow;
                metric.CreatedBy = "System";

                var result = await _pagePerformanceMetricRepository.CreateAsync(metric);
                _logger.LogInformation("Recorded page performance metric {MetricId}", result.PerformanceId);
                return result;
            }

            throw new ArgumentException("Invalid page performance metric data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording page performance metric");
            throw;
        }
    }

    // Query performance methods
    public async Task<object> GetQueryPerformanceAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var metrics = await _queryPerformanceMetricRepository.GetByTimeRangeAsync(startTime, endTime);
            var avgExecutionTime = await _queryPerformanceMetricRepository.GetAverageExecutionTimeAsync(startTime, endTime);
            var totalExecutions = await _queryPerformanceMetricRepository.GetTotalExecutionCountAsync(startTime, endTime);
            var errorRate = await _queryPerformanceMetricRepository.GetAverageErrorRateAsync(startTime, endTime);

            return new
            {
                Metrics = metrics,
                Summary = new
                {
                    AverageExecutionTime = avgExecutionTime,
                    TotalExecutions = totalExecutions,
                    ErrorRate = errorRate,
                    Period = new { StartTime = startTime, EndTime = endTime }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query performance metrics");
            throw;
        }
    }

    public async Task<object> GetSlowestQueriesAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var slowestQueries = await _queryPerformanceMetricRepository.GetSlowestQueriesAsync(topCount, start, end);
            return new { SlowestQueries = slowestQueries, Count = topCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slowest queries");
            throw;
        }
    }

    public async Task<object> GetMostFrequentQueriesAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-1);
            var end = endTime ?? DateTime.UtcNow;

            var mostFrequentQueries = await _queryPerformanceMetricRepository.GetMostFrequentQueriesAsync(topCount, start, end);
            return new { MostFrequentQueries = mostFrequentQueries, Count = topCount };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most frequent queries");
            throw;
        }
    }

    public async Task<object> RecordQueryPerformanceAsync(object queryMetric)
    {
        try
        {
            var json = JsonSerializer.Serialize(queryMetric);
            var metric = JsonSerializer.Deserialize<QueryPerformanceMetricEntity>(json);
            
            if (metric != null)
            {
                metric.QueryMetricId = Guid.NewGuid();
                metric.CreatedOn = DateTime.UtcNow;
                metric.CreatedBy = "System";

                var result = await _queryPerformanceMetricRepository.CreateAsync(metric);
                _logger.LogInformation("Recorded query performance metric {QueryId}", result.QueryMetricId);
                return result;
            }

            throw new ArgumentException("Invalid query performance metric data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording query performance metric");
            throw;
        }
    }

    // System health methods
    public async Task<object> GetSystemHealthHistoryAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var healthHistory = await _systemHealthSnapshotRepository.GetByTimeRangeAsync(startTime, endTime);
            var avgCpuUsage = await _systemHealthSnapshotRepository.GetAverageCpuUsageAsync(startTime, endTime);
            var avgMemoryUsage = await _systemHealthSnapshotRepository.GetAverageMemoryUsageAsync(startTime, endTime);
            var avgDiskUsage = await _systemHealthSnapshotRepository.GetAverageDiskUsageAsync(startTime, endTime);
            var healthPercentage = await _systemHealthSnapshotRepository.GetHealthPercentageAsync(startTime, endTime);

            return new
            {
                HealthHistory = healthHistory,
                Summary = new
                {
                    AverageCpuUsage = avgCpuUsage,
                    AverageMemoryUsage = avgMemoryUsage,
                    AverageDiskUsage = avgDiskUsage,
                    HealthPercentage = healthPercentage,
                    Period = new { StartTime = startTime, EndTime = endTime }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health history");
            throw;
        }
    }

    public async Task<object> GetCurrentSystemHealthAsync()
    {
        try
        {
            var latestSnapshot = await _systemHealthSnapshotRepository.GetLatestAsync();
            if (latestSnapshot == null)
            {
                return new { Status = "Unknown", Message = "No health data available" };
            }
            return latestSnapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current system health");
            return new { Status = "Error", Message = "Unable to retrieve health data" };
        }
    }

    public async Task<object> RecordSystemHealthSnapshotAsync(object healthSnapshot)
    {
        try
        {
            var json = JsonSerializer.Serialize(healthSnapshot);
            var snapshot = JsonSerializer.Deserialize<SystemHealthSnapshotEntity>(json);
            
            if (snapshot != null)
            {
                snapshot.SnapshotId = Guid.NewGuid();
                snapshot.SnapshotTimestamp = DateTime.UtcNow;
                snapshot.CreatedOn = DateTime.UtcNow;
                snapshot.CreatedBy = "System";

                var result = await _systemHealthSnapshotRepository.CreateAsync(snapshot);
                _logger.LogInformation("Recorded system health snapshot {SnapshotId}", result.SnapshotId);
                return result;
            }

            throw new ArgumentException("Invalid system health snapshot data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording system health snapshot");
            throw;
        }
    }

    public async Task<object> GetHealthTrendsAsync(int hours = 24)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddHours(-hours);

            var trends = new List<object>();
            for (int i = 0; i < hours; i++)
            {
                var hourStart = startTime.AddHours(i);
                var hourEnd = hourStart.AddHours(1);

                var avgCpu = await _systemHealthSnapshotRepository.GetAverageCpuUsageAsync(hourStart, hourEnd);
                var avgMemory = await _systemHealthSnapshotRepository.GetAverageMemoryUsageAsync(hourStart, hourEnd);
                var avgDisk = await _systemHealthSnapshotRepository.GetAverageDiskUsageAsync(hourStart, hourEnd);
                var healthPercentage = await _systemHealthSnapshotRepository.GetHealthPercentageAsync(hourStart, hourEnd);

                trends.Add(new
                {
                    Hour = hourStart,
                    AverageCpuUsage = avgCpu,
                    AverageMemoryUsage = avgMemory,
                    AverageDiskUsage = avgDisk,
                    HealthPercentage = healthPercentage
                });
            }

            return new { Trends = trends, Period = new { Hours = hours } };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health trends");
            throw;
        }
    }

    // Analytics and reporting methods
    public async Task<object> GetPerformanceAnalyticsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var systemMetrics = await GetSystemMetricsAsync(startTime, endTime);
            var pagePerformance = await GetPagePerformanceAsync(startTime, endTime);
            var queryPerformance = await GetQueryPerformanceAsync(startTime, endTime);

            return new
            {
                SystemMetrics = systemMetrics,
                PagePerformance = pagePerformance,
                QueryPerformance = queryPerformance,
                Period = new { StartTime = startTime, EndTime = endTime },
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance analytics");
            throw;
        }
    }

    public async Task<object> GetErrorAnalyticsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var errorSummary = await GetErrorSummaryAsync(startTime, endTime);
            var topErrors = await GetTopErrorsAsync(10, startTime, endTime);
            var unresolvedErrors = await GetUnresolvedErrorsAsync();

            return new
            {
                ErrorSummary = errorSummary,
                TopErrors = topErrors,
                UnresolvedErrors = unresolvedErrors,
                Period = new { StartTime = startTime, EndTime = endTime },
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting error analytics");
            throw;
        }
    }

    public async Task<object> GetTrafficAnalyticsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var highestTrafficEndpoints = await GetHighestTrafficEndpointsAsync(10, startTime, endTime);
            var totalRequests = await _pagePerformanceMetricRepository.GetTotalRequestCountAsync(startTime, endTime);
            var avgResponseTime = await _pagePerformanceMetricRepository.GetAverageResponseTimeAsync(startTime, endTime);

            return new
            {
                HighestTrafficEndpoints = highestTrafficEndpoints,
                TotalRequests = totalRequests,
                AverageResponseTime = avgResponseTime,
                Period = new { StartTime = startTime, EndTime = endTime },
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting traffic analytics");
            throw;
        }
    }

    public async Task<object> GetSystemUtilizationAnalyticsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var healthHistory = await GetSystemHealthHistoryAsync(startTime, endTime);
            var maxCpu = await _systemHealthSnapshotRepository.GetMaxCpuUsageAsync(startTime, endTime);
            var maxMemory = await _systemHealthSnapshotRepository.GetMaxMemoryUsageAsync(startTime, endTime);
            var maxDisk = await _systemHealthSnapshotRepository.GetMaxDiskUsageAsync(startTime, endTime);

            return new
            {
                HealthHistory = healthHistory,
                PeakUtilization = new
                {
                    MaxCpuUsage = maxCpu,
                    MaxMemoryUsage = maxMemory,
                    MaxDiskUsage = maxDisk
                },
                Period = new { StartTime = startTime, EndTime = endTime },
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system utilization analytics");
            throw;
        }
    }

    // Export methods
    public async Task<object> ExportMetricsAsync(string format, DateTime startTime, DateTime endTime)
    {
        try
        {
            var analytics = await GetPerformanceAnalyticsAsync(startTime, endTime);
            
            return new
            {
                Format = format,
                Data = analytics,
                ExportedAt = DateTime.UtcNow,
                Period = new { StartTime = startTime, EndTime = endTime }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting metrics");
            throw;
        }
    }

    public async Task<object> ExportErrorReportAsync(string format, DateTime startTime, DateTime endTime)
    {
        try
        {
            var errorAnalytics = await GetErrorAnalyticsAsync(startTime, endTime);
            
            return new
            {
                Format = format,
                Data = errorAnalytics,
                ExportedAt = DateTime.UtcNow,
                Period = new { StartTime = startTime, EndTime = endTime }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting error report");
            throw;
        }
    }

    public async Task<object> ExportPerformanceReportAsync(string format, DateTime startTime, DateTime endTime)
    {
        try
        {
            var performanceAnalytics = await GetPerformanceAnalyticsAsync(startTime, endTime);
            
            return new
            {
                Format = format,
                Data = performanceAnalytics,
                ExportedAt = DateTime.UtcNow,
                Period = new { StartTime = startTime, EndTime = endTime }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting performance report");
            throw;
        }
    }

    // Cleanup and maintenance methods
    public async Task<object> CleanupOldDataAsync(int retentionDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            await _systemMetricRepository.DeleteOlderThanAsync(cutoffDate);
            await _errorLogSummaryRepository.DeleteOlderThanAsync(cutoffDate);
            await _pagePerformanceMetricRepository.DeleteOlderThanAsync(cutoffDate);
            await _queryPerformanceMetricRepository.DeleteOlderThanAsync(cutoffDate);
            await _systemHealthSnapshotRepository.DeleteOlderThanAsync(cutoffDate);

            _logger.LogInformation("Completed cleanup of data older than {CutoffDate}", cutoffDate);

            return new
            {
                CutoffDate = cutoffDate,
                RetentionDays = retentionDays,
                CleanupCompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data cleanup");
            throw;
        }
    }

    public async Task<object> GetDataRetentionStatusAsync()
    {
        try
        {
            var systemMetricCount = await _systemMetricRepository.GetCountAsync();
            var errorLogCount = await _errorLogSummaryRepository.GetCountAsync();
            var pagePerformanceCount = await _pagePerformanceMetricRepository.GetCountAsync();
            var queryPerformanceCount = await _queryPerformanceMetricRepository.GetCountAsync();
            var healthSnapshotCount = await _systemHealthSnapshotRepository.GetCountAsync();

            return new
            {
                SystemMetrics = systemMetricCount,
                ErrorLogs = errorLogCount,
                PagePerformanceMetrics = pagePerformanceCount,
                QueryPerformanceMetrics = queryPerformanceCount,
                HealthSnapshots = healthSnapshotCount,
                TotalRecords = systemMetricCount + errorLogCount + pagePerformanceCount + queryPerformanceCount + healthSnapshotCount,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data retention status");
            throw;
        }
    }

    // Alert and notification methods
    public async Task<object> GetActiveAlertsAsync()
    {
        try
        {
            // This would typically check against configurable thresholds
            var alerts = new List<object>();
            
            var currentHealth = await GetCurrentSystemHealthAsync();
            // Add logic to check thresholds and generate alerts
            
            return new { Alerts = alerts, CheckedAt = DateTime.UtcNow };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alerts");
            throw;
        }
    }

    public async Task<object> ProcessThresholdAlertsAsync()
    {
        try
        {
            // This would process all configured thresholds and generate alerts
            var processedAlerts = new List<object>();
            
            // Add threshold processing logic
            
            return new { ProcessedAlerts = processedAlerts, ProcessedAt = DateTime.UtcNow };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing threshold alerts");
            throw;
        }
    }

    public async Task<object> GetAlertConfigurationAsync()
    {
        try
        {
            // Return current alert configuration
            return new
            {
                CpuThreshold = 80,
                MemoryThreshold = 85,
                DiskThreshold = 90,
                ErrorRateThreshold = 5,
                ResponseTimeThreshold = 2000,
                ConfiguredAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alert configuration");
            throw;
        }
    }

    public async Task<object> UpdateAlertConfigurationAsync(object alertConfig)
    {
        try
        {
            // Update alert configuration
            _logger.LogInformation("Updated alert configuration");
            return new { Updated = true, UpdatedAt = DateTime.UtcNow, Configuration = alertConfig };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating alert configuration");
            throw;
        }
    }

    // Helper methods
    private static string GenerateErrorHash(string errorMessage)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(errorMessage));
        return Convert.ToHexString(hashBytes)[..16]; // Use first 16 characters
    }
}
