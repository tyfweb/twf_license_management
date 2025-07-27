using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Generator.Models;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Interface for license generation services
    /// Implementations should focus only on cryptographic key generation and signing
    /// </summary>
    public interface ILicenseGenerator
    {
        /// <summary>
        /// Generates a signed license using the simplified request with only essential data
        /// </summary>
        /// <param name="request">Simplified license generation request with minimal required data</param>
        /// <returns>Signed license ready for deployment</returns>
        Task<SignedLicense> GenerateLicenseAsync(SimplifiedLicenseGenerationRequest request);

        /// <summary>
        /// Exports the public key for license validation
        /// </summary>
        /// <returns>Public key in PEM format</returns>
        string ExportPublicKey();

        /// <summary>
        /// Saves the private key for license generation (keep secure!)
        /// </summary>
        /// <param name="filePath">Path to save the private key</param>
        /// <param name="password">Password to encrypt the private key</param>
        Task SavePrivateKeyAsync(string filePath, string? password = null);

        /// <summary>
        /// Generate a new key pair and save it to storage
        /// </summary>
        void GenerateAndSaveNewKeyPair();

        /// <summary>
        /// Validates if the current keys are properly loaded and ready for use
        /// </summary>
        bool AreKeysReady();
    }
}
