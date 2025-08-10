using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Generator.Models;

namespace TechWayFit.Licensing.Generator.Services
{
    /// <summary>
    /// Stateless interface for license and cryptographic key generation
    /// This service focuses purely on cryptographic operations without any data storage
    /// </summary>
    public interface ILicenseGenerator
    {
        /// <summary>
        /// Generates a signed license using the provided request and private key
        /// This is a stateless operation - all required data must be provided in the request
        /// </summary>
        /// <param name="request">Complete license generation request including private key for signing</param>
        /// <returns>Signed license ready for deployment</returns>
        Task<SignedLicense> GenerateLicenseAsync(SimplifiedLicenseGenerationRequest request);

        /// <summary>
        /// Generates a new RSA key pair for license signing and validation
        /// This is a stateless operation that returns the generated keys without storing them
        /// </summary>
        /// <param name="keySize">Size of the RSA key in bits (default: 2048, recommended: 4096 for production)</param>
        /// <returns>Generated key pair containing both public and private keys</returns>
        KeyGenerationResult GenerateKeyPair(int keySize = 2048);

        /// <summary>
        /// Extracts the public key from a private key PEM string
        /// This is useful for getting the corresponding public key for distribution
        /// </summary>
        /// <param name="privateKeyPem">Private key in PEM format</param>
        /// <returns>Corresponding public key in PEM format</returns>
        string ExtractPublicKeyFromPrivateKey(string privateKeyPem);

        /// <summary>
        /// Validates a private key PEM string to ensure it's valid and can be used for signing
        /// </summary>
        /// <param name="privateKeyPem">Private key in PEM format to validate</param>
        /// <returns>True if the private key is valid and usable</returns>
        bool ValidatePrivateKey(string privateKeyPem);

        /// <summary>
        /// Validates a public key PEM string to ensure it's valid and can be used for verification
        /// </summary>
        /// <param name="publicKeyPem">Public key in PEM format to validate</param>
        /// <returns>True if the public key is valid and usable</returns>
        bool ValidatePublicKey(string publicKeyPem);

        /// <summary>
        /// Encrypts a private key with a password for secure storage
        /// </summary>
        /// <param name="privateKeyPem">Private key in PEM format</param>
        /// <param name="password">Password to encrypt the private key</param>
        /// <returns>Encrypted private key in PEM format</returns>
        string EncryptPrivateKey(string privateKeyPem, string password);

        /// <summary>
        /// Decrypts an encrypted private key using the provided password
        /// </summary>
        /// <param name="encryptedPrivateKeyPem">Encrypted private key in PEM format</param>
        /// <param name="password">Password to decrypt the private key</param>
        /// <returns>Decrypted private key in PEM format</returns>
        string DecryptPrivateKey(string encryptedPrivateKeyPem, string password);
    }
}
