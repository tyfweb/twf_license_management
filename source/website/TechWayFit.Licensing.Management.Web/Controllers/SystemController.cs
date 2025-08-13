using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using TechWayFit.Licensing.Management.Web.Services.Jobs;
using TechWayFit.Licensing.Management.Web.Extensions;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Controller for system administration and scheduled job management
/// </summary>
[Authorize]
[Route("[controller]")]
public class SystemController : BaseController
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    /// <summary>
    /// System administration dashboard
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("System Administration", "fas fa-cogs")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", isActive: true)
            .SetPrimaryAction("Add Scheduled Job", "fas fa-plus", "System", "CreateJob", buttonClass: "btn btn-success")
            .AddDropdownAction("Import Jobs", "fas fa-file-import", "System", "ImportJobs")
            .AddDropdownAction("Export Configuration", "fas fa-file-export", "System", "ExportConfig")
            .AddDropdownAction("System Settings", "fas fa-cog", "Settings", "Index");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Scheduled jobs management
    /// </summary>
    [HttpGet("Jobs")]
    public IActionResult Jobs()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Scheduled Jobs", "fas fa-clock")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Jobs", isActive: true)
            .SetPrimaryAction("Create Job", "fas fa-plus", "System", "CreateJob", buttonClass: "btn btn-success")
            .AddDropdownAction("Bulk Actions", "fas fa-tasks", onClick: "showBulkActionsModal()")
            .AddDropdownAction("Job Templates", "fas fa-file-alt", "System", "JobTemplates")
            .AddDropdownAction("Execution History", "fas fa-history", "System", "ExecutionHistory");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Job schedules configuration
    /// </summary>
    [HttpGet("Schedules")]
    public IActionResult Schedules()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Job Schedules", "fas fa-calendar-alt")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Schedules", isActive: true)
            .SetPrimaryAction("Create Schedule", "fas fa-plus", "System", "CreateSchedule", buttonClass: "btn btn-success")
            .AddDropdownAction("Schedule Templates", "fas fa-file-alt", "System", "ScheduleTemplates")
            .AddDropdownAction("Cron Expression Helper", "fas fa-question-circle", onClick: "showCronHelper()")
            .AddDropdownAction("Import Schedules", "fas fa-file-import", "System", "ImportSchedules");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Job execution monitoring and history
    /// </summary>
    [HttpGet("Execution")]
    public IActionResult Execution()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Job Execution Monitor", "fas fa-chart-line")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Execution", isActive: true)
            .SetPrimaryAction("Refresh", "fas fa-sync", onClick: "refreshExecutionData()", buttonClass: "btn btn-primary")
            .AddDropdownAction("Export Logs", "fas fa-file-export", "System", "ExportLogs")
            .AddDropdownAction("Clear Old Logs", "fas fa-trash", onClick: "showClearLogsModal()")
            .AddDropdownAction("Execution Settings", "fas fa-cog", "System", "ExecutionSettings");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Real-time system monitoring
    /// </summary>
    [HttpGet("Monitoring")]
    public IActionResult Monitoring()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("System Monitoring", "fas fa-desktop")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Monitoring", isActive: true)
            .SetPrimaryAction("Start Monitoring", "fas fa-play", onClick: "startMonitoring()", buttonClass: "btn btn-success")
            .AddDropdownAction("Performance Metrics", "fas fa-tachometer-alt", "System", "Performance")
            .AddDropdownAction("System Health", "fas fa-heartbeat", "System", "Health")
            .AddDropdownAction("Alert Configuration", "fas fa-bell", "System", "Alerts");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Create new scheduled job
    /// </summary>
    [HttpGet("CreateJob")]
    public IActionResult CreateJob()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Create Scheduled Job", "fas fa-plus")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Jobs", "System", "Jobs")
            .AddBreadcrumb("Create", isActive: true);

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Create new schedule
    /// </summary>
    [HttpGet("CreateSchedule")]
    public IActionResult CreateSchedule()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Create Job Schedule", "fas fa-calendar-plus")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Schedules", "System", "Schedules")
            .AddBreadcrumb("Create", isActive: true);

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Redirect to Hangfire Dashboard
    /// </summary>
    [HttpGet("Dashboard")]
    public IActionResult Dashboard()
    {
        return Redirect("/hangfire");
    }

    /// <summary>
    /// Custom Hangfire Dashboard with application layout
    /// </summary>
    [HttpGet("CustomDashboard")]
    public IActionResult CustomDashboard()
    {
        return RedirectToAction("Index", "HangfireDashboard");
    }

    /// <summary>
    /// Trigger a job manually
    /// </summary>
    [HttpPost("TriggerJob")]
    [ValidateAntiForgeryToken]
    public IActionResult TriggerJob(string jobName)
    {
        try
        {
            switch (jobName.ToLowerInvariant())
            {
                case "license-expiry-check":
                    BackgroundJob.Enqueue<LicenseJobService>(x => x.CheckExpiringLicensesAsync());
                    break;
                case "deactivate-expired-licenses":
                    BackgroundJob.Enqueue<LicenseJobService>(x => x.DeactivateExpiredLicensesAsync());
                    break;
                case "audit-cleanup":
                    BackgroundJob.Enqueue<AuditJobService>(x => x.CleanupOldAuditEntriesAsync());
                    break;
                case "database-maintenance":
                    BackgroundJob.Enqueue<SystemMaintenanceJobService>(x => x.PerformDatabaseMaintenanceAsync());
                    break;
                case "system-health-check":
                    BackgroundJob.Enqueue<SystemMaintenanceJobService>(x => x.MonitorSystemHealthAsync());
                    break;
                default:
                    TempData["ErrorMessage"] = $"Unknown job: {jobName}";
                    return RedirectToAction(nameof(Jobs));
            }

            _logger.LogInformation("Manually triggered job: {JobName} by user: {UserName}", 
                jobName, User.Identity?.Name ?? "Unknown");
            
            TempData["SuccessMessage"] = $"Job '{jobName}' has been queued for execution.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering job: {JobName}", jobName);
            TempData["ErrorMessage"] = $"Error triggering job: {ex.Message}";
        }

        return RedirectToAction(nameof(Jobs));
    }

    /// <summary>
    /// Enable or disable a recurring job
    /// </summary>
    [HttpPost("ToggleJob")]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleJob(string jobId, bool enable)
    {
        try
        {
            if (enable)
            {
                // Re-create the recurring job if it was removed
                switch (jobId.ToLowerInvariant())
                {
                    case "license-expiry-check":
                        RecurringJob.AddOrUpdate<LicenseJobService>(jobId, 
                            x => x.CheckExpiringLicensesAsync(), 
                            "0 9 * * *"); // Daily at 9 AM
                        break;
                    case "deactivate-expired-licenses":
                        RecurringJob.AddOrUpdate<LicenseJobService>(jobId, 
                            x => x.DeactivateExpiredLicensesAsync(), 
                            "0 2 * * *"); // Daily at 2 AM
                        break;
                    case "audit-cleanup":
                        RecurringJob.AddOrUpdate<AuditJobService>(jobId, 
                            x => x.CleanupOldAuditEntriesAsync(), 
                            "0 1 * * 0"); // Weekly on Sunday at 1 AM
                        break;
                    case "database-maintenance":
                        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(jobId, 
                            x => x.PerformDatabaseMaintenanceAsync(), 
                            "0 3 * * 0"); // Weekly on Sunday at 3 AM
                        break;
                    case "system-health-check":
                        RecurringJob.AddOrUpdate<SystemMaintenanceJobService>(jobId, 
                            x => x.MonitorSystemHealthAsync(), 
                            "*/30 * * * *"); // Every 30 minutes
                        break;
                    default:
                        TempData["ErrorMessage"] = $"Unknown job ID: {jobId}";
                        return RedirectToAction(nameof(Jobs));
                }
                
                _logger.LogInformation("Enabled recurring job: {JobId}", jobId);
                TempData["SuccessMessage"] = $"Job '{jobId}' has been enabled.";
            }
            else
            {
                RecurringJob.RemoveIfExists(jobId);
                _logger.LogInformation("Disabled recurring job: {JobId}", jobId);
                TempData["SuccessMessage"] = $"Job '{jobId}' has been disabled.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling job: {JobId}", jobId);
            TempData["ErrorMessage"] = $"Error toggling job: {ex.Message}";
        }

        return RedirectToAction(nameof(Jobs));
    }

    /// <summary>
    /// Get job statistics for dashboard
    /// </summary>
    [HttpGet("JobStats")]
    public IActionResult GetJobStats()
    {
        try
        {
            var stats = new
            {
                // These would need to be implemented with proper Hangfire monitoring
                TotalJobs = 12,
                SuccessfulJobs = 10,
                FailedJobs = 1,
                PendingJobs = 1,
                LastRunTime = DateTime.UtcNow.AddHours(-1)
            };

            return Json(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job statistics");
            return StatusCode(500, "Error loading job statistics");
        }
    }
}
