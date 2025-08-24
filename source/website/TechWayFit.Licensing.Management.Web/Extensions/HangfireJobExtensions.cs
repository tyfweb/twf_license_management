using Hangfire;
using Serilog;
using TechWayFit.Licensing.Management.Web.Services.Jobs;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring Hangfire recurring jobs
/// </summary>
public static class HangfireJobExtensions
{
    /// <summary>
    /// Configures all Hangfire recurring jobs for the application
    /// </summary>
    public static void ConfigureRecurringJobs()
    {
        Log.Information("Configuring Hangfire recurring jobs");
        
        // License management jobs
        RecurringJob.AddOrUpdate<LicenseJobService>(
            "license-expiry-check",
            service => service.CheckExpiringLicensesAsync(),
            Cron.Daily(9)); // Run daily at 9 AM
        
        RecurringJob.AddOrUpdate<LicenseJobService>(
            "deactivate-expired-licenses",
            service => service.DeactivateExpiredLicensesAsync(),
            Cron.Daily(2)); // Run daily at 2 AM
        
        RecurringJob.AddOrUpdate<LicenseJobService>(
            "license-usage-reports",
            service => service.GenerateLicenseUsageReportsAsync(),
            Cron.Weekly(DayOfWeek.Monday, 8)); // Run weekly on Monday at 8 AM
        
        // Audit management jobs
        RecurringJob.AddOrUpdate<AuditJobService>(
            "audit-cleanup",
            service => service.CleanupOldAuditEntriesAsync(),
            Cron.Weekly(DayOfWeek.Sunday, 3)); // Run weekly on Sunday at 3 AM
        
        RecurringJob.AddOrUpdate<AuditJobService>(
            "audit-archival",
            service => service.ArchiveOldAuditEntriesAsync(),
            Cron.Monthly(1, 1)); // Run monthly on the 1st at 1 AM
        
        RecurringJob.AddOrUpdate<AuditJobService>(
            "audit-summary-report",
            service => service.GenerateAuditSummaryReportAsync(),
            Cron.Weekly(DayOfWeek.Monday, 10)); // Run weekly on Monday at 10 AM
        
        // System maintenance jobs
        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
            "database-maintenance",
            service => service.PerformDatabaseMaintenanceAsync(),
            Cron.Weekly(DayOfWeek.Saturday, 1)); // Run weekly on Saturday at 1 AM
        
        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
            "cleanup-temp-files",
            service => service.CleanupTemporaryFilesAsync(),
            Cron.Daily(4)); // Run daily at 4 AM
        
        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
            "system-health-monitoring",
            service => service.MonitorSystemHealthAsync(),
            Cron.Hourly()); // Run every hour
        
        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(
            "performance-reports",
            service => service.GeneratePerformanceReportAsync(),
            Cron.Daily(7)); // Run daily at 7 AM
        
        Log.Information("Hangfire recurring jobs configured successfully");
    }
}
