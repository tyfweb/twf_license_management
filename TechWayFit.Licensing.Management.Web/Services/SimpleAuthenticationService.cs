using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TechWayFit.Licensing.WebUI.Models.Authentication;

namespace TechWayFit.Licensing.WebUI.Services
{
    public interface IAuthenticationService
    {
        Task<bool> ValidateUserAsync(string username, string password);
        Task<User?> GetUserAsync(string username);
        Task SignInAsync(HttpContext context, User user, bool rememberMe);
        Task SignOutAsync(HttpContext context);
    }

    public class SimpleAuthenticationService : IAuthenticationService
    {
        private readonly AuthenticationSettings _authSettings;
        private readonly ILogger<SimpleAuthenticationService> _logger;

        public SimpleAuthenticationService(IOptions<AuthenticationSettings> authSettings, ILogger<SimpleAuthenticationService> logger)
        {
            _authSettings = authSettings.Value;
            _logger = logger;
        }

        public Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = _authSettings.Users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);

            _logger.LogInformation("Login attempt for user: {Username}, Success: {Success}", username, user != null);
            return Task.FromResult(user != null);
        }

        public Task<User?> GetUserAsync(string username)
        {
            var user = _authSettings.Users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public async Task SignInAsync(HttpContext context, User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role),
                new("Username", user.Username)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);
            _logger.LogInformation("User {Username} signed in successfully", user.Username);
        }

        public async Task SignOutAsync(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User signed out");
        }
    }
}
