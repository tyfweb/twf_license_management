using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Services.Contracts;

/// <summary>
/// Strategy interface for different license generation types
/// </summary>
public interface ILicenseGenerationStrategy
{
    /// <summary>
    /// The license type this strategy supports
    /// </summary>
    LicenseType SupportedType { get; }

    /// <summary>
    /// Generates a license using this strategy
    /// </summary>
    /// <param name="request">License generation request</param>
    /// <param name="generatedBy">User generating the license</param>
    /// <returns>Generated product license</returns>
    Task<ProductLicense> GenerateAsync(LicenseGenerationRequest request, string generatedBy);

    /// <summary>
    /// Validates whether this strategy can handle the given request
    /// </summary>
    /// <param name="request">License generation request to validate</param>
    /// <returns>True if this strategy can handle the request</returns>
    bool CanHandle(LicenseGenerationRequest request);

    /// <summary>
    /// Gets the recommended settings for this license type
    /// </summary>
    /// <returns>Dictionary of recommended settings</returns>
    Dictionary<string, object> GetRecommendedSettings();
}
