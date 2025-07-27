using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of key management service for product licensing
/// Manages RSA key pairs for products used in license generation
/// </summary>
public class KeyManagementService : IKeyManagementService
{
    private readonly ILicenseGenerator _licenseGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeyManagementService> _logger;
    private readonly string _keyStorePath;

    public KeyManagementService(
        ILicenseGenerator licenseGenerator,
        IConfiguration configuration,
        ILogger<KeyManagementService> logger)
    {
        _licenseGenerator = licenseGenerator ?? throw new ArgumentNullException(nameof(licenseGenerator));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get key storage path from configuration or use default
        _keyStorePath = _configuration["Licensing:KeyStorePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Keys");
        
        // Ensure directory exists
        Directory.CreateDirectory(_keyStorePath);
        
        _logger.LogInformation("KeyManagementService initialized with storage path: {KeyStorePath}", _keyStorePath);
    }

    public async Task<string> GetPrivateKeyAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var privateKeyPath = GetPrivateKeyPath(productId);
            
            if (!File.Exists(privateKeyPath))
            {
                _logger.LogWarning("Private key not found for product {ProductId} at path {Path}", productId, privateKeyPath);
                throw new FileNotFoundException($"Private key not found for product {productId}");
            }

            var encryptedPrivateKey = await File.ReadAllTextAsync(privateKeyPath);
            
            // Check if the key is encrypted (contains encryption headers)
            if (encryptedPrivateKey.Contains("ENCRYPTED"))
            {
                // For now, we'll assume keys are not encrypted
                // In production, you would get the encryption password from secure storage
                _logger.LogWarning("Encrypted private key detected for product {ProductId} - decryption not implemented", productId);
                throw new NotImplementedException("Encrypted private key decryption not implemented yet");
            }

            _logger.LogDebug("Successfully retrieved private key for product {ProductId}", productId);
            return encryptedPrivateKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving private key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task StorePrivateKeyAsync(string productId, string privateKeyPem, string? encryptionPassword = null)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(privateKeyPem))
            throw new ArgumentException("PrivateKeyPem cannot be null or empty", nameof(privateKeyPem));

        try
        {
            // Validate the private key before storing
            if (!_licenseGenerator.ValidatePrivateKey(privateKeyPem))
            {
                throw new ArgumentException("Invalid private key provided");
            }

            var privateKeyPath = GetPrivateKeyPath(productId);
            var publicKeyPath = GetPublicKeyPath(productId);

            // Encrypt the private key if password is provided
            string keyToStore = privateKeyPem;
            if (!string.IsNullOrEmpty(encryptionPassword))
            {
                keyToStore = _licenseGenerator.EncryptPrivateKey(privateKeyPem, encryptionPassword);
                _logger.LogInformation("Private key encrypted for product {ProductId}", productId);
            }

            // Store private key
            await File.WriteAllTextAsync(privateKeyPath, keyToStore);

            // Extract and store corresponding public key
            var publicKeyPem = _licenseGenerator.ExtractPublicKeyFromPrivateKey(privateKeyPem);
            await File.WriteAllTextAsync(publicKeyPath, publicKeyPem);

            _logger.LogInformation("Successfully stored private and public keys for product {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing private key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<string> GenerateKeyPairForProductAsync(string productId, int keySize = 2048, string? encryptionPassword = null)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            _logger.LogInformation("Generating new key pair for product {ProductId} with key size {KeySize}", productId, keySize);

            // Generate new key pair using the license generator
            var keyGenerationResult = _licenseGenerator.GenerateKeyPair(keySize);

            // Store the keys
            await StorePrivateKeyAsync(productId, keyGenerationResult.PrivateKeyPem, encryptionPassword);

            _logger.LogInformation("Successfully generated and stored key pair {KeyId} for product {ProductId}", 
                keyGenerationResult.KeyId, productId);

            return keyGenerationResult.PublicKeyPem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating key pair for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<string> GetPublicKeyAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var publicKeyPath = GetPublicKeyPath(productId);
            
            if (!File.Exists(publicKeyPath))
            {
                _logger.LogWarning("Public key not found for product {ProductId} at path {Path}", productId, publicKeyPath);
                throw new FileNotFoundException($"Public key not found for product {productId}");
            }

            var publicKey = await File.ReadAllTextAsync(publicKeyPath);
            
            // Validate the public key
            if (!_licenseGenerator.ValidatePublicKey(publicKey))
            {
                _logger.LogError("Invalid public key found for product {ProductId}", productId);
                throw new InvalidOperationException($"Invalid public key for product {productId}");
            }

            _logger.LogDebug("Successfully retrieved public key for product {ProductId}", productId);
            return publicKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> HasValidKeysAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return false;

        try
        {
            var privateKeyPath = GetPrivateKeyPath(productId);
            var publicKeyPath = GetPublicKeyPath(productId);

            // Check if both files exist
            if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
            {
                return false;
            }

            // Validate private key
            var privateKey = await File.ReadAllTextAsync(privateKeyPath);
            if (privateKey.Contains("ENCRYPTED"))
            {
                // For encrypted keys, we can't validate without the password
                // For now, just check if files exist
                _logger.LogDebug("Product {ProductId} has encrypted keys - assuming valid", productId);
                return true;
            }

            if (!_licenseGenerator.ValidatePrivateKey(privateKey))
            {
                _logger.LogWarning("Invalid private key found for product {ProductId}", productId);
                return false;
            }

            // Validate public key
            var publicKey = await File.ReadAllTextAsync(publicKeyPath);
            if (!_licenseGenerator.ValidatePublicKey(publicKey))
            {
                _logger.LogWarning("Invalid public key found for product {ProductId}", productId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking keys for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<string> RotateKeysAsync(string productId, int keySize = 2048, string? encryptionPassword = null)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            _logger.LogInformation("Rotating keys for product {ProductId}", productId);

            // Archive existing keys if they exist
            await ArchiveExistingKeysAsync(productId);

            // Generate new key pair
            var publicKey = await GenerateKeyPairForProductAsync(productId, keySize, encryptionPassword);

            _logger.LogInformation("Successfully rotated keys for product {ProductId}", productId);
            return publicKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating keys for product {ProductId}", productId);
            throw;
        }
    }

    #region Private Helper Methods

    private string GetPrivateKeyPath(string productId)
    {
        return Path.Combine(_keyStorePath, $"private_key_{productId}.pem");
    }

    private string GetPublicKeyPath(string productId)
    {
        return Path.Combine(_keyStorePath, $"public_key_{productId}.pem");
    }

    private Task ArchiveExistingKeysAsync(string productId)
    {
        try
        {
            var privateKeyPath = GetPrivateKeyPath(productId);
            var publicKeyPath = GetPublicKeyPath(productId);
            
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            
            if (File.Exists(privateKeyPath))
            {
                var archivePath = Path.Combine(_keyStorePath, $"archived_private_key_{productId}_{timestamp}.pem");
                File.Move(privateKeyPath, archivePath);
                _logger.LogInformation("Archived existing private key for product {ProductId} to {ArchivePath}", productId, archivePath);
            }

            if (File.Exists(publicKeyPath))
            {
                var archivePath = Path.Combine(_keyStorePath, $"archived_public_key_{productId}_{timestamp}.pem");
                File.Move(publicKeyPath, archivePath);
                _logger.LogInformation("Archived existing public key for product {ProductId} to {ArchivePath}", productId, archivePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving existing keys for product {ProductId}", productId);
            // Don't throw - archiving is not critical for key rotation
            return Task.CompletedTask;
        }
    }

    #endregion
}
