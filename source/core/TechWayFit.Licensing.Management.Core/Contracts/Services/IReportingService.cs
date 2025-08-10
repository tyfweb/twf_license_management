using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Report;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for reporting and analytics in license management
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates license usage report
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="consumerId">Filter by consumer ID</param>
    /// <returns>License usage report</returns>
    Task<LicenseUsageReport> GenerateLicenseUsageReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? productId = null,
        string? consumerId = null);

    /// <summary>
    /// Generates license expiration report
    /// </summary>
    /// <param name="daysAhead">Number of days ahead to check for expirations</param>
    /// <param name="productId">Filter by product ID</param>
    /// <returns>License expiration report</returns>
    Task<LicenseExpirationReport> GenerateLicenseExpirationReportAsync(
        int daysAhead = 90,
        string? productId = null);

    /// <summary>
    /// Generates revenue report based on license sales
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <param name="productId">Filter by product ID</param>
    /// <param name="groupBy">Group by option (Month, Quarter, Year)</param>
    /// <returns>Revenue report</returns>
    Task<RevenueReport> GenerateRevenueReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? productId = null,
        ReportGroupBy groupBy = ReportGroupBy.Month);

    /// <summary>
    /// Generates product performance report
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <returns>Product performance report</returns>
    Task<ProductPerformanceReport> GenerateProductPerformanceReportAsync(
        DateTime fromDate,
        DateTime toDate);

    /// <summary>
    /// Generates consumer activity report
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <param name="consumerId">Filter by consumer ID</param>
    /// <returns>Consumer activity report</returns>
    Task<ConsumerActivityReport> GenerateConsumerActivityReportAsync(
        DateTime fromDate,
        DateTime toDate,
        string? consumerId = null);

    /// <summary>
    /// Generates compliance audit report
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <param name="complianceType">Type of compliance to check</param>
    /// <returns>Compliance audit report</returns>
    Task<ComplianceAuditReport> GenerateComplianceAuditReportAsync(
        DateTime fromDate,
        DateTime toDate,
        ComplianceType complianceType = ComplianceType.All);

    /// <summary>
    /// Generates license violations report
    /// </summary>
    /// <param name="fromDate">Start date for report</param>
    /// <param name="toDate">End date for report</param>
    /// <param name="violationType">Filter by violation type</param>
    /// <returns>License violations report</returns>
    Task<LicenseViolationsReport> GenerateLicenseViolationsReportAsync(
        DateTime fromDate,
        DateTime toDate,
        ViolationType? violationType = null);

    /// <summary>
    /// Generates dashboard analytics
    /// </summary>
    /// <param name="dateRange">Date range for analytics</param>
    /// <returns>Dashboard analytics data</returns>
    Task<DashboardAnalytics> GenerateDashboardAnalyticsAsync(DateRange dateRange = DateRange.Last30Days);

    /// <summary>
    /// Generates custom report based on parameters
    /// </summary>
    /// <param name="reportParameters">Custom report parameters</param>
    /// <returns>Custom report data</returns>
    Task<CustomReportData> GenerateCustomReportAsync(CustomReportParameters reportParameters);

    /// <summary>
    /// Exports report to specified format
    /// </summary>
    /// <param name="reportData">Report data to export</param>
    /// <param name="format">Export format (PDF, Excel, CSV)</param>
    /// <returns>Exported report as byte array</returns>
    Task<byte[]> ExportReportAsync(object reportData, ExportFormat format);

    /// <summary>
    /// Schedules a recurring report
    /// </summary>
    /// <param name="reportSchedule">Report schedule configuration</param>
    /// <returns>Scheduled report ID</returns>
    Task<string> ScheduleReportAsync(ReportSchedule reportSchedule);

    /// <summary>
    /// Gets scheduled reports
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>List of scheduled reports</returns>
    Task<IEnumerable<ReportSchedule>> GetScheduledReportsAsync(bool? isActive = null);

    /// <summary>
    /// Updates a scheduled report
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <param name="reportSchedule">Updated schedule configuration</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateScheduledReportAsync(string scheduleId, ReportSchedule reportSchedule);

    /// <summary>
    /// Deletes a scheduled report
    /// </summary>
    /// <param name="scheduleId">Schedule ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteScheduledReportAsync(string scheduleId);

    /// <summary>
    /// Gets report execution history
    /// </summary>
    /// <param name="reportType">Filter by report type</param>
    /// <param name="fromDate">Start date filter</param>
    /// <param name="toDate">End date filter</param>
    /// <returns>List of report executions</returns>
    Task<IEnumerable<ReportExecution>> GetReportExecutionHistoryAsync(
        string? reportType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
}
