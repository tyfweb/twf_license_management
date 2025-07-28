using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    /// <summary>
    /// Home controller for basic navigation and system information
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main landing page with step-by-step progress
        /// </summary>
        public IActionResult Index()
        {
            // Pass authentication status to view
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.Username = User.Identity?.Name ?? "Anonymous";
            ViewBag.UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "None";
            
            return View();
        }

        /// <summary>
        /// Bootstrap test page to verify CSS classes
        /// </summary>
        public IActionResult BootstrapTest()
        {
            return View();
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
