using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Core.Services;

namespace TechWayFit.Licensing.Validation.Services.Strategies
{
    /// <summary>
    /// Interface for type-specific license validation strategies
    /// </summary>
    public interface ILicenseValidationStrategy
    {
        /// <summary>
        /// Gets the license type this strategy handles
        /// </summary>
        LicenseType SupportedLicenseType { get; }

        /// <summary>
        /// Validates a license using type-specific rules
        /// </summary>
        /// <param name="license">The license to validate</param>
        /// <param name="options">Validation options</param>
        /// <returns>Validation result with type-specific checks applied</returns>
        Task<LicenseValidationResult> ValidateAsync(License license, LicenseValidationOptions options);

        /// <summary>
        /// Validates business rules specific to this license type
        /// </summary>
        /// <param name="license">The license to validate</param>
        /// <returns>Validation result for business rules</returns>
        Task<LicenseValidationResult> ValidateBusinessRulesAsync(License license);

        /// <summary>
        /// Gets type-specific error messages and warnings
        /// </summary>
        /// <param name="license">The license being validated</param>
        /// <returns>List of validation messages</returns>
        Task<IEnumerable<string>> GetValidationMessagesAsync(License license);
    }
}
