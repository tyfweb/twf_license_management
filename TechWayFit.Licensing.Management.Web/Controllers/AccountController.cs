using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.WebUI.Models.Authentication;
using TechWayFit.Licensing.WebUI.Services;

namespace TechWayFit.Licensing.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthenticationService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = await _authService.ValidateUserAsync(model.Username, model.Password);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var user = await _authService.GetUserAsync(model.Username);
            if (user != null)
            {
                await _authService.SignInAsync(HttpContext, user, model.RememberMe);
                
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                
                return RedirectToAction("Index", "License");
            }

            ModelState.AddModelError(string.Empty, "Login failed.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync(HttpContext);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
