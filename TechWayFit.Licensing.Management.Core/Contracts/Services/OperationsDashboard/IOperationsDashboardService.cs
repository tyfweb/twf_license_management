namespace TechWayFit.Licensing.Management.Core.Contracts.Services.OperationsDashboard;

/// <summary>
/// Service contract for operations dashboard functionality
/// </summary>
public interface IOperationsDashboardService
{
    // Dashboard overview methods
    Task<object> GetDashboardOverviewAsync(DateTime? startTime = null, DateTime? endTime = null);
    Task<object> GetSystemHealthStatusAsync();
    Task<object> GetRealTimeMetricsAsync();
    
    // System metrics methods
    Task<object> GetSystemMetricsAsync(DateTime startTime, DateTime endTime);
    Task<object> GetSystemPerformanceTrendsAsync(int days = 7);
    Task<object> RecordSystemMetricAsync(object systemMetric);
    Task<object> RecordSystemMetricsBulkAsync(IEnumerable<object> systemMetrics);
    
    // Error tracking methods
    Task<object> GetErrorSummaryAsync(DateTime startTime, DateTime endTime);
    Task<object> GetTopErrorsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null);
    Task<object> GetUnresolvedErrorsAsync();
    Task<object> RecordErrorAsync(object errorLog);
    Task<object> RecordErrorsBulkAsync(IEnumerable<object> errorLogs);
    Task<object> MarkErrorAsResolvedAsync(string errorHash, string resolvedBy);
    
    // Performance metrics methods
    Task<object> GetPagePerformanceAsync(DateTime startTime, DateTime endTime);
    Task<object> GetSlowestEndpointsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null);
    Task<object> GetHighestTrafficEndpointsAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null);
    Task<object> RecordPagePerformanceAsync(object pageMetric);
    Task<object> RecordPagePerformanceBulkAsync(IEnumerable<object> pageMetrics);
    
    // Query performance methods
    Task<object> GetQueryPerformanceAsync(DateTime startTime, DateTime endTime);
    Task<object> GetSlowestQueriesAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null);
    Task<object> GetMostFrequentQueriesAsync(int topCount = 10, DateTime? startTime = null, DateTime? endTime = null);
    Task<object> RecordQueryPerformanceAsync(object queryMetric);
    Task<object> RecordQueryPerformanceBulkAsync(IEnumerable<object> queryMetrics);
    
    // System health methods
    Task<object> GetSystemHealthHistoryAsync(DateTime startTime, DateTime endTime);
    Task<object> GetCurrentSystemHealthAsync();
    Task<object> RecordSystemHealthSnapshotAsync(object healthSnapshot);
    Task<object> RecordSystemHealthSnapshotsBulkAsync(IEnumerable<object> healthSnapshots);
    Task<object> GetHealthTrendsAsync(int hours = 24);
    
    // Analytics and reporting methods
    Task<object> GetPerformanceAnalyticsAsync(DateTime startTime, DateTime endTime);
    Task<object> GetErrorAnalyticsAsync(DateTime startTime, DateTime endTime);
    Task<object> GetTrafficAnalyticsAsync(DateTime startTime, DateTime endTime);
    Task<object> GetSystemUtilizationAnalyticsAsync(DateTime startTime, DateTime endTime);
    
    // Export methods
    Task<object> ExportMetricsAsync(string format, DateTime startTime, DateTime endTime);
    Task<object> ExportErrorReportAsync(string format, DateTime startTime, DateTime endTime);
    Task<object> ExportPerformanceReportAsync(string format, DateTime startTime, DateTime endTime);
    
    // Cleanup and maintenance methods
    Task<object> CleanupOldDataAsync(int retentionDays = 30);
    Task<object> GetDataRetentionStatusAsync();
    
    // Alert and notification methods
    Task<object> GetActiveAlertsAsync();
    Task<object> ProcessThresholdAlertsAsync();
    Task<object> GetAlertConfigurationAsync();
    Task<object> UpdateAlertConfigurationAsync(object alertConfig);
}
