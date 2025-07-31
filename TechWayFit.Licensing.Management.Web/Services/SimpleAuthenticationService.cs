
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CoreServices = TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.WebUI.Models.Authentication;

namespace TechWayFit.Licensing.WebUI.Services
{
    

    public class AuthenticationManager 
    { 
        private readonly ILogger<AuthenticationManager> _logger;
        private readonly CoreServices.IAuthenticationService _authenticationService;

        public AuthenticationManager(CoreServices.IAuthenticationService authenticationService, ILogger<AuthenticationManager> logger)
        { 
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
       
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var isValid = await _authenticationService.ValidateUserAsync(username,password);
           
            if (!isValid)
            {
                _logger.LogWarning("Invalid login attempt for user: {Username}", username);
                return false;
            }

            _logger.LogInformation("User {Username} validated successfully", username);
            return true;
        }

        public async Task<User?> GetUserAsync(string username)
        {
            var user = await _authenticationService.GetUserAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return null;
            }

            _logger.LogInformation("User {Username} retrieved successfully", username);
            return new User
            {
                Name = user.FullName,
                Username = user.UserName,
                Roles = user.Roles.Select(r => r.RoleName)
            };
        }

        public async Task SignInAsync(HttpContext context, User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Name),
                new("Username", user.Username)
            };
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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
