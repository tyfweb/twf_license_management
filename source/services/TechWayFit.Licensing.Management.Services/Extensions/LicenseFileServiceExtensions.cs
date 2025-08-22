using Microsoft.Extensions.DependencyInjection;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Services.Implementations.License;

namespace TechWayFit.Licensing.Management.Services.Extensions;

/// <summary>
/// Extension methods for registering license file and activation services in the DI container
/// </summary>
public static class LicenseFileServiceExtensions
{
    /// <summary>
    /// Add license file generation services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLicenseFileServices(this IServiceCollection services)
    {
        services.AddScoped<ILicenseFileService, LicenseFileService>();
        
        return services;
    }

    /// <summary>
    /// Add license activation services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLicenseActivationServices(this IServiceCollection services)
    {
        services.AddScoped<ILicenseActivationService, LicenseActivationService>();
        
        return services;
    }

    /// <summary>
    /// Add all license management services (Task 5 implementation)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTask5LicenseServices(this IServiceCollection services)
    {
        services.AddLicenseFileServices();
        services.AddLicenseActivationServices();
        
        return services;
    }
}
