using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Utilities;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of the Product Activation service for managing product key activations
/// </summary>
public class ProductActivationService : IProductActivationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductActivationService> _logger;

    public ProductActivationService(
        IUnitOfWork unitOfWork,
        ILogger<ProductActivationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new product key registration
    /// </summary>
    public async Task<ProductKeyRegistrationResult> CreateProductKeyAsync(CreateProductKeyRequest request, string createdBy)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(createdBy)) throw new ArgumentException("Created by cannot be null or empty", nameof(createdBy));

        try
        {
            // Generate unique product key
            var productKey = ProductKeyGenerator.GenerateProductKey();
            
            _logger.LogInformation("Generated product key for Product {ProductId}, Consumer {ConsumerId}", 
                request.ProductId, request.ConsumerId);

            // Create ProductLicense core model
            var license = new ProductLicense
            {
                LicenseId = Guid.NewGuid(),
                LicenseCode = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                ProductId = request.ProductId,
                ConsumerId = request.ConsumerId,
                ProductTierId = request.ProductTierId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                LicenseKey = productKey, // Store the product key as license key
                PublicKey = string.Empty, // Not needed for product keys
                KeyGeneratedAt = DateTime.UtcNow,
                Status = LicenseStatus.Pending, // Start as pending until activated
                Metadata = request.Metadata ?? new Dictionary<string, object>(),
                IssuedBy = createdBy,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                UpdatedBy = createdBy,
                UpdatedOn = DateTime.UtcNow
            };

            // Save to database through repository
            var createdLicense = await _unitOfWork.Licenses.AddAsync(license);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created product key registration with License ID: {LicenseId}", createdLicense.LicenseId);

            return ProductKeyRegistrationResult.CreateSuccess(
                productKey, 
                createdLicense.LicenseId, 
                request.MaxActivations, 
                request.ValidFrom, 
                request.ValidTo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product key registration for Product {ProductId}, Consumer {ConsumerId}", 
                request.ProductId, request.ConsumerId);
            
            return ProductKeyRegistrationResult.CreateFailure($"Failed to create product key registration: {ex.Message}");
        }
    }

    /// <summary>
    /// Activates a product key with machine details
    /// </summary>
    public async Task<ProductActivationResult> ActivateProductKeyAsync(ProductActivationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            _logger.LogInformation("Activating product key: {ProductKey} for Machine: {MachineId}",
                request.ProductKey, request.MachineId);

            // Normalize and validate product key format
            if (!ProductKeyGenerator.TryNormalizeProductKey(request.ProductKey, out var normalizedKey))
            {
                return ProductActivationResult.CreateFailure("Invalid product key format");
            }

            // Validate product key
            var validation = await ValidateProductKeyAsync(normalizedKey);
            if (!validation.IsValid || !validation.CanActivate)
            {
                return ProductActivationResult.CreateFailure(validation.ErrorMessage ?? "Product key cannot be activated");
            }

            // Check if machine is already activated for this product key
            var existingActivation = await _unitOfWork.ProductActivations
                .GetByProductKeyAndMachineAsync(normalizedKey, request.MachineId);

            if (existingActivation != null && existingActivation.Status == ProductActivationStatus.Active)
            {
                return ProductActivationResult.CreateFailure("Product key is already activated on this machine");
            }

            // Get the license
            var license = await _unitOfWork.Licenses.GetByLicenseKeyAsync(normalizedKey);
            if (license == null)
            {
                return ProductActivationResult.CreateFailure("License not found");
            }

            // Calculate activation end date from license validity
            var activationEndDate = license.ValidTo;

            // Generate unique activation signature
            var activationSignature = GenerateActivationSignature(normalizedKey, request.MachineId, license.Id);

            // Create new activation record
            var activation = new ProductActivation
            {
                Id = Guid.NewGuid(),
                LicenseId = license.Id,
                FormattedProductKey = normalizedKey,
                MaxActivations = validation.MaxActivations,
                ProductKey = normalizedKey,
                MachineId = request.MachineId,
                MachineName = request.MachineName,
                MachineFingerprint = request.MachineFingerprint,
                IpAddress = request.IpAddress,
                ActivationDate = DateTime.UtcNow,
                ActivationEndDate = activationEndDate,
                ActivationSignature = activationSignature,
                Status = ProductActivationStatus.Active,
                ActivationData = request.ActivationData != null ? JsonSerializer.Serialize(request.ActivationData) : "{}",
                IsActive = true,
                IsDeleted = false,
                CreatedBy = "SYSTEM",
                CreatedOn = DateTime.UtcNow,
                UpdatedBy = "SYSTEM",
                UpdatedOn = DateTime.UtcNow
            };

            // Update license status to Active
            license.Status = LicenseStatus.Active;
            license.UpdatedBy = "SYSTEM";
            license.UpdatedOn = DateTime.UtcNow;

            // Save changes
            await _unitOfWork.ProductActivations.AddAsync(activation);
            await _unitOfWork.Licenses.UpdateAsync(license.Id, license);
            await _unitOfWork.SaveChangesAsync();

            // Create activation details
            var activationDetails = new ProductActivationDetails
            {
                ActivationId = activation.Id,
                LicenseId = license.Id,
                ProductKey = normalizedKey,
                MachineId = request.MachineId,
                MachineName = request.MachineName,
                ActivationDate = activation.ActivationDate,
                ActivationEndDate = activationEndDate,
                Status = ProductActivationStatus.Active,
                ActivationSignature = activationSignature
            };

            _logger.LogInformation("Successfully activated product key: {ProductKey} with signature: {Signature}",
                normalizedKey, activationSignature);

            return ProductActivationResult.CreateSuccess(
                activationSignature,
                activation.Id,
                activationEndDate,
                activationDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product key: {ProductKey}", request.ProductKey);
            return ProductActivationResult.CreateFailure($"Failed to activate product key: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves activation details using signature
    /// </summary>
    public async Task<ProductActivationDetails?> GetActivationBySignatureAsync(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("Signature cannot be null or empty", nameof(signature));

        try
        {
            var activation = await _unitOfWork.ProductActivations.GetBySignatureAsync(signature);
            if (activation == null)
                return null;

            return new ProductActivationDetails
            {
                ActivationId = activation.Id,
                LicenseId = activation.LicenseId,
                ProductKey = activation.ProductKey,
                MachineId = activation.MachineId,
                MachineName = activation.MachineName,
                ActivationDate = activation.ActivationDate,
                ActivationEndDate = activation.ActivationEndDate,
                Status = activation.Status,
                ActivationSignature = activation.ActivationSignature,
                LastHeartbeat = activation.LastHeartbeat
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activation by signature: {Signature}", signature);
            throw;
        }
    }

    /// <summary>
    /// Validates if a product key exists and can be activated
    /// </summary>
    public async Task<ProductKeyValidationResult> ValidateProductKeyAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            return ProductKeyValidationResult.Invalid("Product key cannot be empty");

        try
        {
            // Normalize product key
            if (!ProductKeyGenerator.TryNormalizeProductKey(productKey, out var normalizedKey))
            {
                return ProductKeyValidationResult.Invalid("Invalid product key format");
            }

            // Get license by product key
            var license = await _unitOfWork.Licenses.GetByLicenseKeyAsync(normalizedKey);
            if (license == null)
            {
                return ProductKeyValidationResult.Invalid("Product key not found");
            }

            // Check if this is a product key license (XXXX-XXXX-XXXX-XXXX format)
            if (!ProductKeyGenerator.ValidateProductKeyFormat(license.LicenseKey))
            {
                return ProductKeyValidationResult.Invalid("Not a product key license");
            }

            // Check license validity dates
            var now = DateTime.UtcNow;
            if (now < license.ValidFrom || now > license.ValidTo)
            {
                return ProductKeyValidationResult.Invalid("Product key has expired or is not yet valid");
            }

            // Check license status
            if (license.Status == LicenseStatus.Revoked || license.Status == LicenseStatus.Expired)
            {
                return ProductKeyValidationResult.Invalid($"Product key is {license.Status.ToString().ToLower()}");
            }

            // Get activation configuration
            var activationConfig = await _unitOfWork.ProductActivations.GetConfigurationByLicenseIdAsync(license.Id);
            if (activationConfig == null)
            {
                return ProductKeyValidationResult.Invalid("Activation configuration not found");
            }

            // Count current active activations
            var currentActivations = await _unitOfWork.ProductActivations.CountActiveActivationsByProductKeyAsync(normalizedKey);

            return ProductKeyValidationResult.Valid(
                license.Id,
                currentActivations,
                activationConfig.MaxActivations,
                license.ValidFrom,
                license.ValidTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product key: {ProductKey}", productKey);
            return ProductKeyValidationResult.Invalid("An error occurred while validating the product key");
        }
    }

    /// <summary>
    /// Deactivates a product key activation
    /// </summary>
    public async Task<bool> DeactivateProductKeyAsync(string signature, string deactivatedBy, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("Signature cannot be null or empty", nameof(signature));
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("DeactivatedBy cannot be null or empty", nameof(deactivatedBy));

        try
        {
            var activation = await _unitOfWork.ProductActivations.GetBySignatureAsync(signature);
            if (activation == null)
            {
                _logger.LogWarning("Activation not found for signature: {Signature}", signature);
                return false;
            }

            if (activation.Status != ProductActivationStatus.Active)
            {
                _logger.LogWarning("Activation {ActivationId} is not active, current status: {Status}",
                    activation.Id, activation.Status);
                return false;
            }

            // Update activation status
            activation.Status = ProductActivationStatus.Inactive;
            activation.DeactivationDate = DateTime.UtcNow;
            activation.DeactivationReason = reason;
            activation.DeactivatedBy = deactivatedBy;
            activation.UpdatedBy = deactivatedBy;
            activation.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.ProductActivations.UpdateAsync(activation.Id, activation);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deactivated activation {ActivationId} by {DeactivatedBy}",
                activation.Id, deactivatedBy);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product key with signature: {Signature}", signature);
            throw;
        }
    }

    /// <summary>
    /// Gets all activations for a product key
    /// </summary>
    public async Task<IEnumerable<ProductActivationDetails>> GetActivationsByProductKeyAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            throw new ArgumentException("Product key cannot be null or empty", nameof(productKey));

        try
        {
            if (!ProductKeyGenerator.TryNormalizeProductKey(productKey, out var normalizedKey))
            {
                return Enumerable.Empty<ProductActivationDetails>();
            }

            var activations = await _unitOfWork.ProductActivations.GetByProductKeyAsync(normalizedKey);

            return activations.Select(a => new ProductActivationDetails
            {
                ActivationId = a.Id,
                LicenseId = a.LicenseId,
                ProductKey = a.ProductKey,
                MachineId = a.MachineId,
                MachineName = a.MachineName,
                ActivationDate = a.ActivationDate,
                ActivationEndDate = a.ActivationEndDate,
                Status = a.Status,
                ActivationSignature = a.ActivationSignature,
                LastHeartbeat = a.LastHeartbeat
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activations for product key: {ProductKey}", productKey);
            throw;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Generates a unique product key that doesn't exist in the database
    /// </summary>
    private async Task<string> GenerateUniqueProductKeyAsync()
    {
        const int maxAttempts = 10;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var productKey = ProductKeyGenerator.GenerateProductKey();
            
            // Check if this key already exists
            var existingLicense = await _unitOfWork.Licenses.GetByLicenseKeyAsync(productKey);
            if (existingLicense == null)
            {
                return productKey;
            }
            
            _logger.LogDebug("Product key collision detected on attempt {Attempt}: {ProductKey}", attempt + 1, productKey);
        }
        
        throw new InvalidOperationException($"Failed to generate unique product key after {maxAttempts} attempts");
    }

    /// <summary>
    /// Generates a unique activation signature
    /// </summary>
    private static string GenerateActivationSignature(string productKey, string machineId, Guid licenseId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{productKey}|{machineId}|{licenseId}|{timestamp}";
        
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        var signature = Convert.ToBase64String(hashBytes);
        
        // Make it URL-safe and remove padding
        return signature.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    #endregion
}
