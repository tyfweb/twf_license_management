using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Web.Models.Authentication;
using TechWayFit.Licensing.Management.Web.Services;
using TechWayFit.Licensing.Management.Web.Extensions;

namespace TechWayFit.Licensing.Management.Web.Controllers
{
    
    public class AccountController : BaseController
    {
        private readonly AuthenticationManager _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthenticationManager authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
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

            var isValid = await _authService.ValidateUserAsync(model.Username, model.Password);

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
    }
}
