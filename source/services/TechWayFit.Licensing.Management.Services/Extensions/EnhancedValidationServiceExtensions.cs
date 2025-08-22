using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Services.Implementations.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;

namespace TechWayFit.Licensing.Management.Services.Extensions;

/// <summary>
/// Extension methods for registering enhanced license validation services
/// </summary>
public static class EnhancedValidationServiceExtensions
{
    /// <summary>
    /// Registers the enhanced license validation service
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddEnhancedLicenseValidation(this IServiceCollection services)
    {
        // Register the enhanced validation service
        services.AddScoped<LicenseValidationEnhancementService>();

        return services;
    }

    /// <summary>
    /// Configures enhanced license validation with custom logger
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddEnhancedLicenseValidation(
        this IServiceCollection services, 
        Action<EnhancedValidationOptions> configure)
    {
        var options = new EnhancedValidationOptions();
        configure(options);

        // Register the enhanced validation service
        services.AddScoped<LicenseValidationEnhancementService>();

        return services;
    }
}

/// <summary>
/// Configuration options for enhanced license validation
/// </summary>
public class EnhancedValidationOptions
{
    /// <summary>
    /// Enable detailed logging for validation operations
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Enable business rules validation
    /// </summary>
    public bool EnableBusinessRulesValidation { get; set; } = true;

    /// <summary>
    /// Enable expiration warnings
    /// </summary>
    public bool EnableExpirationWarnings { get; set; } = true;

    /// <summary>
    /// Days before expiration to start showing warnings
    /// </summary>
    public int ExpirationWarningDays { get; set; } = 30;

    /// <summary>
    /// Days before expiration to show urgent warnings
    /// </summary>
    public int UrgentExpirationWarningDays { get; set; } = 7;

    /// <summary>
    /// Enable license type specific validation
    /// </summary>
    public bool EnableLicenseTypeValidation { get; set; } = true;

    /// <summary>
    /// Enable usage and activation validation
    /// </summary>
    public bool EnableUsageValidation { get; set; } = true;
}
