using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Extensions;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// Custom Hangfire Dashboard Controller that integrates with the application layout
/// </summary>
[Authorize]
public class HangfireDashboardController : BaseController
{
    private readonly ILogger<HangfireDashboardController> _logger;

    public HangfireDashboardController(ILogger<HangfireDashboardController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Custom Hangfire dashboard with application layout
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Job Dashboard", "fas fa-tachometer-alt")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Job Dashboard", isActive: true)
            .SetPrimaryAction("View Full Dashboard", "fas fa-external-link-alt", "/hangfire", buttonClass: "btn btn-primary")
            .AddDropdownAction("Servers", "fas fa-server", "/hangfire/servers")
            .AddDropdownAction("Recurring Jobs", "fas fa-repeat", "/hangfire/recurring");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Embedded dashboard in a frame
    /// </summary>
    [HttpGet]
    public IActionResult Embedded()
    {
        var pageHeader = PageHeaderExtensions
            .CreatePageHeader("Embedded Dashboard", "fas fa-desktop")
            .AddBreadcrumb("Home", "Home", "Index")
            .AddBreadcrumb("System", "System", "Index")
            .AddBreadcrumb("Dashboard", isActive: true)
            .SetPrimaryAction("Full Screen", "fas fa-expand", "/hangfire", buttonClass: "btn btn-primary")
            .AddDropdownAction("Jobs", "fas fa-tasks", "/hangfire/jobs/enqueued")
            .AddDropdownAction("Servers", "fas fa-server", "/hangfire/servers");

        ViewBag.PageHeader = pageHeader;
        return View();
    }

    /// <summary>
    /// Full-screen dashboard redirect
    /// </summary>
    [HttpGet("FullScreen")]
    public IActionResult FullScreen()
    {
        return Redirect("/hangfire");
    }
}
