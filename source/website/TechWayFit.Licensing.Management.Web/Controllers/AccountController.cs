using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Models.Authentication;
using TechWayFit.Licensing.Management.Web.Services;
using TechWayFit.Licensing.Management.Web.Extensions;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Contracts.Services;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    
    public class AccountController : BaseController
    {
        private readonly AuthenticationManager _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly ITenantScope _tenantScope;
        private readonly ITenantService _tenantService;

        public AccountController(
            AuthenticationManager authService, 
            ILogger<AccountController> logger, 
            ITenantScope tenantScope,
            ITenantService tenantService)
        {
            _authService = authService;
            _logger = logger;
            _tenantScope = tenantScope;
            _tenantService = tenantService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = false;
            // Execute seeding within system tenant scope
            using var systemScope = _tenantScope.CreateSystemScope();            
                isValid = await _authService.ValidateUserAsync(model.Username, model.Password);
            
            if (isValid)
            {
                var user = await _authService.GetUserAsync(model.Username);
                if (user != null)
                {
                    await _authService.SignInAsync(HttpContext, user, model.RememberMe);
                    _logger.LogUserAuthentication(model.Username, true, HttpContext.Connection.RemoteIpAddress?.ToString());

                    // Redirect to return URL or home
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            _logger.LogUserAuthentication(model.Username, false, HttpContext.Connection.RemoteIpAddress?.ToString());
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";
            await _authService.SignOutAsync(HttpContext);
            _logger.LogInformation("User {Username} logged out successfully", username);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Switch current tenant context for administrators
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public IActionResult SwitchTenant([FromBody] SwitchTenantRequest request)
        {
            try
            {
                if (request?.TenantId == null)
                {
                    return Json(new { success = false, message = "Invalid tenant ID" });
                }

                // Store the selected tenant ID in session for admin impersonation
                HttpContext.Session.SetString("AdminSelectedTenantId", request.TenantId.Value.ToString());

                _logger.LogInformation("Administrator {Username} switched to tenant {TenantId}", 
                    User.Identity?.Name, request.TenantId);

                return Json(new { success = true, message = "Tenant switched successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching tenant for user {Username}", User.Identity?.Name);
                return Json(new { success = false, message = "Failed to switch tenant" });
            }
        }

        /// <summary>
        /// Get available tenants for administrator tenant switching
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAvailableTenants()
        {
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                _logger.LogInformation("Retrieved {Count} tenants from service", tenants.Count());

                var tenantList = tenants.Select(t => new 
                { 
                    Id = t.TenantId, 
                    Name = t.TenantName ?? $"Tenant {t.TenantId}", 
                    Code = t.TenantCode ?? "N/A"
                }).ToList();

                // Log the first tenant for debugging
                if (tenantList.Any())
                {
                    var firstTenant = tenantList.First();
                    _logger.LogInformation("First tenant: Id={Id}, Name={Name}, Code={Code}", 
                        firstTenant.Id, firstTenant.Name, firstTenant.Code);
                }

                // Get currently selected tenant
                var currentTenantId = HttpContext.Session.GetString("AdminSelectedTenantId");

                return Json(new { success = true, tenants = tenantList, currentTenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tenants for user {Username}", User.Identity?.Name);
                return Json(new { success = false, message = "Failed to load tenants" });
            }
        }

        /// <summary>
        /// Clear tenant selection (return to default admin view)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public IActionResult ClearTenantSelection()
        {
            try
            {
                HttpContext.Session.Remove("AdminSelectedTenantId");
                
                _logger.LogInformation("Administrator {Username} cleared tenant selection", User.Identity?.Name);
                
                return Json(new { success = true, message = "Tenant selection cleared" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing tenant selection for user {Username}", User.Identity?.Name);
                return Json(new { success = false, message = "Failed to clear tenant selection" });
            }
        }
    }

    /// <summary>
    /// Request model for tenant switching
    /// </summary>
    public class SwitchTenantRequest
    {
        public Guid? TenantId { get; set; }
    }
}
