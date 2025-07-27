namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing cryptographic keys used in license generation
/// This service manages private keys for products and provides them to the license generator
/// </summary>
public interface IKeyManagementService
{
    /// <summary>
    /// Retrieves the private key for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Private key in PEM format</returns>
    Task<string> GetPrivateKeyAsync(string productId);

    /// <summary>
    /// Stores a private key for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="privateKeyPem">Private key in PEM format</param>
    /// <param name="encryptionPassword">Optional password to encrypt the key</param>
    Task StorePrivateKeyAsync(string productId, string privateKeyPem, string? encryptionPassword = null);

    /// <summary>
    /// Generates a new key pair for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="keySize">RSA key size in bits (default: 2048)</param>
    /// <param name="encryptionPassword">Optional password to encrypt the private key</param>
    /// <returns>Public key in PEM format (private key is stored automatically)</returns>
    Task<string> GenerateKeyPairForProductAsync(string productId, int keySize = 2048, string? encryptionPassword = null);

    /// <summary>
    /// Gets the public key for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Public key in PEM format</returns>
    Task<string> GetPublicKeyAsync(string productId);

    /// <summary>
    /// Validates if a product has valid keys configured
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if valid keys exist</returns>
    Task<bool> HasValidKeysAsync(string productId);

    /// <summary>
    /// Rotates keys for a product (generates new keys and archives old ones)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="keySize">RSA key size in bits (default: 2048)</param>
    /// <param name="encryptionPassword">Optional password to encrypt the private key</param>
    /// <returns>New public key in PEM format</returns>
    Task<string> RotateKeysAsync(string productId, int keySize = 2048, string? encryptionPassword = null);
}
