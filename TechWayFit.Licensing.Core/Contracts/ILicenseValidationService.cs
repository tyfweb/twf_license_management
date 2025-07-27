using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Core.Contracts
{
    /// <summary>
    /// Defines the contract for license validation services.
    /// This service provides lean validation focused on cryptographic integrity and temporal validation.
    /// </summary>
    public interface ILicenseValidationService
    {
        /// <summary>
        /// Validates a signed license with cryptographic signature verification and date validation.
        /// </summary>
        /// <param name="signedLicense">The signed license to validate</param>
        /// <param name="publicKey">The RSA public key for signature verification</param>
        /// <param name="options">Optional validation options</param>
        /// <returns>License validation result containing decoded license data and validation status</returns>
        Task<LicenseValidationResult> ValidateAsync(SignedLicense signedLicense, string publicKey, LicenseValidationOptions? options = null);

        /// <summary>
        /// Validates a license from JSON string format.
        /// </summary>
        /// <param name="licenseJson">The license in JSON format</param>
        /// <param name="publicKey">The RSA public key for signature verification</param>
        /// <param name="options">Optional validation options</param>
        /// <returns>License validation result</returns>
        Task<LicenseValidationResult> ValidateFromJsonAsync(string licenseJson, string publicKey, LicenseValidationOptions? options = null);

        /// <summary>
        /// Validates a license from file.
        /// </summary>
        /// <param name="filePath">Path to the license file</param>
        /// <param name="publicKeyPath">Path to the public key file</param>
        /// <param name="options">Optional validation options</param>
        /// <returns>License validation result</returns>
        Task<LicenseValidationResult> ValidateFromFileAsync(string filePath, string publicKeyPath, LicenseValidationOptions? options = null);
    }
}