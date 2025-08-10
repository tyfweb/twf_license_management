using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.ViewModels.License;
using TechWayFit.Licensing.Management.Web.ViewModels.Home;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    /// <summary>
    /// Home controller for basic navigation and system information
    /// </summary>
[Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductLicenseService _productLicenseService;

        public HomeController(ILogger<HomeController> logger, IProductLicenseService productLicenseService)
        {
            _productLicenseService = productLicenseService;
            _logger = logger;
        }

        /// <summary>
        /// Main landing page with step-by-step progress
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Pass authentication status to view
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.Username = User.Identity?.Name ?? "Anonymous";
            ViewBag.UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "None";
            var licenseStats=await _productLicenseService.GetLicenseUsageStatisticsAsync();
            var licenseModel= new LicenseStatsViewModel
            {
                TotalLicenses = licenseStats.TotalLicenses,
                ActiveLicenses = licenseStats.ActiveLicenses,
                ExpiringLicenses = licenseStats.ExpiringInNext30Days,
                ExpiredLicenses = licenseStats.ExpiredLicenses,
                PendingApprovals = licenseStats.LicensesByStatus.ContainsKey(Licensing.Core.Models.LicenseStatus.Pending) ? licenseStats.LicensesByStatus[Licensing.Core.Models.LicenseStatus.Pending] : 0,

            };
            var viewModel = new LicenseHomeViewModel
            {
                LicenseStats = licenseModel,
               // RecentAuditLogs = await _productLicenseService.GetRecentAuditLogsAsync(10) // Fetch last 10 audit logs
            };
            return View(viewModel);
        }


        /// <summary>
        /// Error page
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
