using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.WebUI.Models.Authentication;
using TechWayFit.Licensing.WebUI.Services;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    
    public class AccountController : Controller
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
                    _logger.LogInformation("User {Username} logged in successfully", model.Username);

                    // Redirect to return URL or home
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            _logger.LogWarning("Failed login attempt for user: {Username}", model.Username);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync(HttpContext);
            _logger.LogInformation("User logged out");
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
