using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Database-based implementation of key management service for product licensing
/// Manages RSA key pairs for products used in license generation
/// </summary>
public class DatabaseKeyManagementService : IKeyManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILicenseGenerator _licenseGenerator;
    private readonly ILogger<DatabaseKeyManagementService> _logger;

    public DatabaseKeyManagementService(
        IUnitOfWork unitOfWork,
        ILicenseGenerator licenseGenerator,
        ILogger<DatabaseKeyManagementService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _licenseGenerator = licenseGenerator ?? throw new ArgumentNullException(nameof(licenseGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogInformation("DatabaseKeyManagementService initialized");
    }

    public async Task<string> GetPrivateKeyAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        try
        {
            var activeKeys = await _unitOfWork.ProductKeys.GetActiveKeysByProductIdAsync(productId);
            
            if (activeKeys == null)
            {
                _logger.LogWarning("No active keys found for product {ProductId}", productId);
                throw new InvalidOperationException($"No active keys found for product {productId}");
            }

            // For now, assume keys are not encrypted
            // In production, you would decrypt the private key here
            if (string.IsNullOrEmpty(activeKeys.PrivateKeyEncrypted))
            {
                _logger.LogError("Private key is empty for product {ProductId}", productId);
                throw new InvalidOperationException($"Private key is empty for product {productId}");
            }

            _logger.LogDebug("Successfully retrieved private key for product {ProductId}", productId);
            return activeKeys.PrivateKeyEncrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving private key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task StorePrivateKeyAsync(Guid productId, string privateKeyPem, string? encryptionPassword = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(privateKeyPem))
            throw new ArgumentException("PrivateKeyPem cannot be null or empty", nameof(privateKeyPem));

        try
        {
            // Validate the private key before storing
            if (!_licenseGenerator.ValidatePrivateKey(privateKeyPem))
            {
                throw new ArgumentException("Invalid private key provided");
            }

            // Extract public key from private key
            var publicKeyPem = _licenseGenerator.ExtractPublicKeyFromPrivateKey(privateKeyPem);

            // Encrypt the private key if password is provided
            string keyToStore = privateKeyPem;
            if (!string.IsNullOrEmpty(encryptionPassword))
            {
                keyToStore = _licenseGenerator.EncryptPrivateKey(privateKeyPem, encryptionPassword);
                _logger.LogInformation("Private key encrypted for product {ProductId}", productId);
            }

            // Create new ProductKeys entity
            var productKeys = new ProductKeys
            {
                ProductId = productId,
                PrivateKeyEncrypted = keyToStore,
                PublicKey = publicKeyPem,
                KeySize = 2048, // Default, could be extracted from key
                Algorithm = "RSA",
                KeyGeneratedAt = DateTime.UtcNow,
                KeyVersion = 1,
                IsActive = true
            };

            // Deactivate existing keys and add new ones
            await _unitOfWork.ProductKeys.RotateKeysAsync(productId, productKeys);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Private key stored successfully for product {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing private key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<string> GenerateKeyPairForProductAsync(Guid productId, int keySize = 2048, string? encryptionPassword = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        try
        {
            _logger.LogInformation("Generating new key pair for product {ProductId} with key size {KeySize}", productId, keySize);

            // Generate new key pair
            var keyPair = _licenseGenerator.GenerateKeyPair(keySize);
            var privateKeyPem = keyPair.PrivateKeyPem;
            var publicKeyPem = keyPair.PublicKeyPem;

            // Store the private key (this will also store the public key)
            await StorePrivateKeyAsync(productId, privateKeyPem, encryptionPassword);

            _logger.LogInformation("Key pair generated and stored successfully for product {ProductId}", productId);
            return publicKeyPem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating key pair for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<string> GetPublicKeyAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        try
        {
            var activeKeys = await _unitOfWork.ProductKeys.GetActiveKeysByProductIdAsync(productId);
            
            if (activeKeys == null)
            {
                _logger.LogWarning("No active keys found for product {ProductId}", productId);
                throw new InvalidOperationException($"No active keys found for product {productId}");
            }

            if (string.IsNullOrEmpty(activeKeys.PublicKey))
            {
                _logger.LogError("Public key is empty for product {ProductId}", productId);
                throw new InvalidOperationException($"Public key is empty for product {productId}");
            }

            _logger.LogDebug("Successfully retrieved public key for product {ProductId}", productId);
            return activeKeys.PublicKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public key for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> HasValidKeysAsync(Guid productId)
    {
        if (productId == Guid.Empty)
            return false;

        try
        {
            var hasKeys = await _unitOfWork.ProductKeys.HasActiveKeysAsync(productId);
            _logger.LogDebug("Product {ProductId} has valid keys: {HasKeys}", productId, hasKeys);
            return hasKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product {ProductId} has valid keys", productId);
            return false;
        }
    }

    public async Task<string> RotateKeysAsync(Guid productId, int keySize = 2048, string? encryptionPassword = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        try
        {
            _logger.LogInformation("Rotating keys for product {ProductId}", productId);

            // Generate new key pair and store it (this will automatically deactivate old keys)
            var publicKey = await GenerateKeyPairForProductAsync(productId, keySize, encryptionPassword);

            _logger.LogInformation("Keys rotated successfully for product {ProductId}", productId);
            return publicKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating keys for product {ProductId}", productId);
            throw;
        }
    }
}
