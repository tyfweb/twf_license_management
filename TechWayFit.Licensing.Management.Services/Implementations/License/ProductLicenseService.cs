using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using System.Text.Json;
using CoreModels = TechWayFit.Licensing.Core.Models;
using ManagementModels = TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of the Product License service
/// </summary>
public class ProductLicenseService : IProductLicenseService
{
    private readonly IProductLicenseRepository _productLicenseRepository;
    private readonly ILicenseGenerator _licenseGenerator;
    private readonly IKeyManagementService _keyManagementService;
    private readonly ILogger<ProductLicenseService> _logger;

    public ProductLicenseService(
        IProductLicenseRepository productLicenseRepository,
        ILicenseGenerator licenseGenerator,
        IKeyManagementService keyManagementService,
        ILogger<ProductLicenseService> logger)
    {
        _productLicenseRepository = productLicenseRepository ?? throw new ArgumentNullException(nameof(productLicenseRepository));
        _licenseGenerator = licenseGenerator ?? throw new ArgumentNullException(nameof(licenseGenerator));
        _keyManagementService = keyManagementService ?? throw new ArgumentNullException(nameof(keyManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a new product license
    /// </summary>
    public async Task<ProductLicense> GenerateLicenseAsync(LicenseGenerationRequest request, string generatedBy)
    {
        _logger.LogInformation("Generating license for product: {ProductId}, consumer: {ConsumerId}", 
            request.ProductId, request.ConsumerId);

        // Input validation
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(generatedBy))
            throw new ArgumentException("GeneratedBy cannot be null or empty", nameof(generatedBy));

        try
        {
            // Get or generate private key for the product
            var privateKey = await _keyManagementService.GetPrivateKeyAsync(request.ProductId);
            if (string.IsNullOrEmpty(privateKey))
            {
                _logger.LogInformation("No private key found for product {ProductId}, generating new key pair", request.ProductId);
                var publicKey = await _keyManagementService.GenerateKeyPairForProductAsync(request.ProductId);
                privateKey = await _keyManagementService.GetPrivateKeyAsync(request.ProductId);
                _logger.LogInformation("Generated new key pair for product {ProductId}, public key length: {PublicKeyLength}", 
                    request.ProductId, publicKey.Length);
            }

            // Create license generation request for the Generator
            var generationRequest = new SimplifiedLicenseGenerationRequest
            {
                ProductId = request.ProductId,
                LicensedTo = request.ConsumerId, // Map ConsumerId to LicensedTo
                ValidFrom = DateTime.UtcNow,
                ValidTo = request.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                CustomData = request.Metadata ?? new Dictionary<string, object>(),
                PrivateKeyPem = privateKey,
                // TODO: Map additional fields when needed:
                // ProductName, ContactPerson, ContactEmail, Features, etc.
            };

            // Generate cryptographically signed license
            var signedLicense = await _licenseGenerator.GenerateLicenseAsync(generationRequest);
            
            var licenseId = Guid.NewGuid().ToString();

            // Create license entity for database storage
            var licenseEntity = new ProductLicenseEntity
            {
                LicenseId = licenseId,
                ProductId = request.ProductId,
                ConsumerId = request.ConsumerId,
                LicenseKey = signedLicense.LicenseData, // Store the signed license data
                ValidFrom = generationRequest.ValidFrom,
                ValidTo = generationRequest.ValidTo,
                Status = ManagementModels.LicenseStatus.Active.ToString(),
                MetadataJson = SerializeMetadata(request.Metadata ?? new Dictionary<string, object>()),
                CreatedBy = generatedBy,
                CreatedOn = DateTime.UtcNow,
                UpdatedBy = generatedBy,
                UpdatedOn = DateTime.UtcNow
            };

            // TODO: Set additional properties when available in entity:
            // - TierId, MaxUsers, MaxDevices, AllowOfflineUsage, AllowVirtualization, Notes, CustomPropertiesJson
            _logger.LogWarning("Some license properties not set - entity structure incomplete");

            // Save to repository
            var createdEntity = await _productLicenseRepository.AddAsync(licenseEntity);
            
            // Map to model
            var result = createdEntity.ToModel();
            
            _logger.LogInformation("Successfully generated cryptographically signed license with ID: {LicenseId}", result.LicenseId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating license for product: {ProductId}", request.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Gets a license by license key
    /// </summary>
    public async Task<ProductLicense?> GetLicenseByKeyAsync(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));

        try
        {
            // TODO: Implement GetByLicenseKeyAsync in repository
            _logger.LogWarning("GetLicenseByKeyAsync using search - GetByLicenseKeyAsync repository method missing");
            
            var searchRequest = new SearchRequest<ProductLicenseEntity>
            {
                Filters = new List<Func<ProductLicenseEntity, bool>>
                {
                    l => l.LicenseKey == licenseKey
                }
            };
            
            var searchResult = await _productLicenseRepository.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();
            
            return entity?.ToModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by key: {LicenseKey}", licenseKey);
            throw;
        }
    }

    /// <summary>
    /// Validates a license
    /// </summary>
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, string productId, bool checkActivation = true)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var license = await GetLicenseByKeyAsync(licenseKey);

            if (license == null)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "License not found",
                    ValidationDetails = new Dictionary<string, object>
                    {
                        ["licenseKey"] = licenseKey,
                        ["productId"] = productId,
                        ["validatedAt"] = DateTime.UtcNow
                    }
                };
            }

            // Check product match
            if (license.LicenseConsumer?.Product?.ProductId != productId)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "License not valid for this product",
                    License = license,
                    ValidationDetails = new Dictionary<string, object>
                    {
                        ["expectedProductId"] = productId,
                        ["actualProductId"] = license.LicenseConsumer?.Product?.ProductId ?? "unknown",
                        ["validatedAt"] = DateTime.UtcNow
                    }
                };
            }

            // Check license status and validity
            var now = DateTime.UtcNow;
            if (license.Status != LicenseStatus.Active || now < license.ValidFrom || now > license.ValidTo)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"License is not active or expired. Status: {license.Status}, Valid: {license.ValidFrom:yyyy-MM-dd} to {license.ValidTo:yyyy-MM-dd}",
                    License = license,
                    ValidationDetails = new Dictionary<string, object>
                    {
                        ["status"] = license.Status.ToString(),
                        ["validFrom"] = license.ValidFrom,
                        ["validTo"] = license.ValidTo,
                        ["currentDate"] = now,
                        ["validatedAt"] = DateTime.UtcNow
                    }
                };
            }

            // License is valid
            return new LicenseValidationResult
            {
                IsValid = true,
                License = license,
                ValidationDetails = new Dictionary<string, object>
                {
                    ["validatedAt"] = DateTime.UtcNow,
                    ["status"] = license.Status.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license: {LicenseKey}", licenseKey);
            
            return new LicenseValidationResult
            {
                IsValid = false,
                ErrorMessage = "License validation failed due to system error",
                ValidationDetails = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["validatedAt"] = DateTime.UtcNow
                }
            };
        }
    }

    #region TODO: Missing Interface Methods - Require Implementation

    public async Task<ProductLicense> UpdateLicenseAsync(string licenseId, LicenseUpdateRequest request, string updatedBy)
    {
        // TODO: Implement when entity properties are available
        _logger.LogWarning("UpdateLicenseAsync not fully implemented - entity properties missing");
        await Task.CompletedTask;
        throw new NotImplementedException("UpdateLicenseAsync not implemented - entity structure incomplete");
    }

    public async Task<ProductLicense> RegenerateLicenseKeyAsync(string licenseId, string regeneratedBy, string reason)
    {
        // TODO: Implement
        _logger.LogWarning("RegenerateLicenseKeyAsync not implemented");
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<ProductLicense?> GetLicenseByIdAsync(string licenseId)
    {
        // TODO: Implement
        _logger.LogWarning("GetLicenseByIdAsync not implemented");
        await Task.CompletedTask;
        return null;
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByConsumerAsync(string consumerId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        // TODO: Implement when repository methods are available
        _logger.LogWarning("GetLicensesByConsumerAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<ProductLicense>();
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByProductAsync(string productId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        // TODO: Implement when repository methods are available
        _logger.LogWarning("GetLicensesByProductAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<ProductLicense>();
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysAhead = 30)
    {
        // TODO: Implement when repository methods are available
        _logger.LogWarning("GetExpiringLicensesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<ProductLicense>();
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiredLicensesAsync()
    {
        // TODO: Implement
        _logger.LogWarning("GetExpiredLicensesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<ProductLicense>();
    }

    public async Task<bool> ActivateLicenseAsync(string licenseId, ActivationInfo activationInfo)
    {
        // TODO: Implement
        _logger.LogWarning("ActivateLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeactivateLicenseAsync(string licenseId, string deactivatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("DeactivateLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> SuspendLicenseAsync(string licenseId, string suspendedBy, string reason, DateTime? suspendUntil = null)
    {
        // TODO: Implement
        _logger.LogWarning("SuspendLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> RevokeLicenseAsync(string licenseId, string revokedBy, string reason)
    {
        // TODO: Implement
        _logger.LogWarning("RevokeLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> RenewLicenseAsync(string licenseId, DateTime newExpiryDate, string renewedBy)
    {
        // TODO: Implement
        _logger.LogWarning("RenewLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> UpdateLicenseStatusAsync(string licenseId, LicenseStatus status, string updatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("UpdateLicenseStatusAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeleteLicenseAsync(string licenseId, string deletedBy)
    {
        // TODO: Implement
        _logger.LogWarning("DeleteLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> LicenseExistsAsync(string licenseId)
    {
        // TODO: Implement
        _logger.LogWarning("LicenseExistsAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<ValidationResult> ValidateLicenseDataAsync(ProductLicense license)
    {
        // TODO: Implement
        _logger.LogWarning("ValidateLicenseDataAsync not implemented");
        await Task.CompletedTask;
        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateLicenseGenerationRequestAsync(LicenseGenerationRequest request)
    {
        // TODO: Implement
        _logger.LogWarning("ValidateLicenseGenerationRequestAsync not implemented");
        await Task.CompletedTask;
        return ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateLicenseUpdateRequestAsync(string licenseId, LicenseUpdateRequest request)
    {
        // TODO: Implement
        _logger.LogWarning("ValidateLicenseUpdateRequestAsync not implemented");
        await Task.CompletedTask;
        return ValidationResult.Success();
    }

    public async Task<IEnumerable<ProductLicense>> GetAllLicensesAsync(LicenseStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        // TODO: Implement
        _logger.LogWarning("GetAllLicensesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<ProductLicense>();
    }

    public async Task<int> GetLicenseCountAsync(LicenseStatus? status = null, string? searchTerm = null)
    {
        // TODO: Implement
        _logger.LogWarning("GetLicenseCountAsync not implemented");
        await Task.CompletedTask;
        return 0;
    }

    public async Task<LicenseUsageStatistics> GetLicenseUsageStatisticsAsync(string? productId = null, string? consumerId = null)
    {
        // TODO: Implement
        _logger.LogWarning("GetLicenseUsageStatisticsAsync not implemented");
        await Task.CompletedTask;
        return new LicenseUsageStatistics();
    }

    public async Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(string licenseId)
    {
        // TODO: Implement
        _logger.LogWarning("GetLicenseAuditHistoryAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<LicenseAuditEntry>();
    }

    #endregion

    #region Private Helper Methods

    private static string SerializeMetadata(Dictionary<string, object> metadata)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Serialize(metadata);
        }
        catch
        {
            return "{}";
        }
    }

    #endregion
}
