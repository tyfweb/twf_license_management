using Microsoft.AspNetCore.Authentication.Cookies;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Web.Services;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for configuring authentication and authorization services
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds authentication and authorization services to the service collection
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The configured service collection</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure authentication with cookie support
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        // Add authorization services
        services.AddAuthorization();

        // Register tenant scope infrastructure for system operations
        services.AddSingleton<ITenantScope, TenantScope>();
        services.AddScoped<IUserContext, TenantAwareUserContext>();
        services.AddScoped<AuthenticationManager>();

        return services;
    }
}
