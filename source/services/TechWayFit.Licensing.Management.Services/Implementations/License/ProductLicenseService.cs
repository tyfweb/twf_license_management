using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Services.Factories;
using System.Text.Json;
using CoreModels = TechWayFit.Licensing.Core.Models;
using ManagementModels = TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of the Product License service
/// </summary>
public class ProductLicenseService : IProductLicenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILicenseGenerationFactory _licenseGenerationFactory;
    private readonly ILogger<ProductLicenseService> _logger;
    private readonly LicenseValidationEnhancementService? _enhancedValidationService;

    public ProductLicenseService(
        IUnitOfWork unitOfWork,
        ILicenseGenerationFactory licenseGenerationFactory,
        ILogger<ProductLicenseService> logger,
        LicenseValidationEnhancementService? enhancedValidationService = null)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _licenseGenerationFactory = licenseGenerationFactory ?? throw new ArgumentNullException(nameof(licenseGenerationFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _enhancedValidationService = enhancedValidationService;
    }

    /// <summary>
    /// Generates a new product license using the appropriate strategy pattern
    /// </summary>
    public async Task<ProductLicense> GenerateLicenseAsync(LicenseGenerationRequest request, string generatedBy)
    {
        _logger.LogInformation("Generating {LicenseModel} license for product: {ProductId}, consumer: {ConsumerId}",
            request.LicenseModel, request.ProductId, request.ConsumerId);

        try
        {
            // Use the factory to generate the license with the appropriate strategy
            var result = await _licenseGenerationFactory.GenerateAsync(request, generatedBy);

            _logger.LogInformation("Successfully generated {LicenseModel} license with ID: {LicenseId} and key: {LicenseKey}", 
                request.LicenseModel, result.LicenseId, result.LicenseKey);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {LicenseModel} license for product: {ProductId}", 
                request.LicenseModel, request.ProductId);
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
            _logger.LogDebug("Getting license by key: {LicenseKey}", licenseKey);

            // Use the repository method directly
            var entity = await _unitOfWork.Licenses.GetByLicenseKeyAsync(licenseKey);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by key: {LicenseKey}", licenseKey);
            throw;
        }
    }

    /// <summary>
    /// Gets a license by license code (user-friendly identifier)
    /// </summary>
    public async Task<ProductLicense?> GetLicenseByCodeAsync(string licenseCode)
    {
        if (string.IsNullOrWhiteSpace(licenseCode))
            throw new ArgumentException("LicenseCode cannot be null or empty", nameof(licenseCode));

        try
        {
            _logger.LogInformation("Getting license by code: {LicenseCode}", licenseCode);

            // Use the repository method directly
            var entity = await _unitOfWork.Licenses.GetByLicenseCodeAsync(licenseCode);

            if (entity != null)
            {
                _logger.LogDebug("Found license {LicenseId} for code {LicenseCode}", entity.LicenseId, licenseCode);
            }
            else
            {
                _logger.LogWarning("No license found for code: {LicenseCode}", licenseCode);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by code: {LicenseCode}", licenseCode);
            throw;
        }
    }

    /// <summary>
    /// Gets a license by either license key or license code
    /// </summary>
    public async Task<ProductLicense?> GetLicenseByKeyOrCodeAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

        try
        {
            _logger.LogInformation("Getting license by key or code: {Identifier}", identifier);

            // Use the repository method that searches both key and code
            var license = await _unitOfWork.Licenses.GetByKeyOrCodeAsync(identifier);

            if (license != null)
            {
                _logger.LogDebug("Found license {LicenseId} for identifier {Identifier}", license.LicenseId, identifier);
            }
            else
            {
                _logger.LogWarning("No license found for identifier: {Identifier}", identifier);
            }

            return license;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by key or code: {Identifier}", identifier);
            throw;
        }
    }

    /// <summary>
    /// Validates a license
    /// </summary>
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, Guid productId, bool checkActivation = true)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));
        if (Guid.Empty.Equals(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var license = await GetLicenseByKeyAsync(licenseKey);

            if (license == null)
            {
                return LicenseValidationResult.Failure(LicenseStatus.NotFound, "License not found");
            }

            // Check product match
            if (license.LicenseConsumer?.Product?.ProductId != productId)
            {
                return LicenseValidationResult.Failure(LicenseStatus.NotFound,
                "License does not match the specified product");
            }

            // Check license status and validity
            var now = DateTime.UtcNow;
            if (license.Status != LicenseStatus.Active || now < license.ValidFrom || now > license.ValidTo)
            {
                return LicenseValidationResult.Failure(LicenseStatus.Expired,
                    "License is not active or is outside the valid date range");
            }

            // License is valid
            return LicenseValidationResult.Success(license.ToLicenseModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license: {LicenseKey}", licenseKey);

            return LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, 
                    "An error occurred while validating the license. Please try again later.");
        }
    }

    /// <summary>
    /// Performs enhanced validation of a license with detailed business rules and usage analysis
    /// </summary>
    /// <param name="licenseKey">License key to validate</param>
    /// <param name="productId">Product ID to validate against</param>
    /// <param name="includeDetailedAnalysis">Whether to include detailed validation analysis</param>
    /// <returns>Enhanced validation result with detailed information</returns>
    public async Task<EnhancedLicenseValidationResult?> ValidateLicenseWithEnhancedRulesAsync(
        string licenseKey, 
        Guid productId, 
        bool includeDetailedAnalysis = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));
            if (Guid.Empty.Equals(productId))
                throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

            _logger.LogInformation("Starting enhanced validation for license key: {LicenseKey}, product: {ProductId}", 
                licenseKey.Substring(0, Math.Min(8, licenseKey.Length)) + "...", productId);

            // First, get the license
            var license = await GetLicenseByKeyAsync(licenseKey);
            if (license == null)
            {
                _logger.LogWarning("License not found during enhanced validation: {LicenseKey}", licenseKey);
                return new EnhancedLicenseValidationResult
                {
                    LicenseKey = licenseKey,
                    IsValid = false,
                    ValidationMessages = new List<string> { "License not found" },
                    BusinessRuleViolations = new List<string> { "License key does not exist in the system" },
                    Warnings = new List<string>()
                };
            }

            // Check product match
            if (license.LicenseConsumer?.Product?.ProductId != productId)
            {
                _logger.LogWarning("Product mismatch during enhanced validation. Expected: {ExpectedProductId}, Found: {ActualProductId}", 
                    productId, license.LicenseConsumer?.Product?.ProductId);
                return new EnhancedLicenseValidationResult
                {
                    LicenseId = license.LicenseId,
                    LicenseKey = licenseKey,
                    LicenseType = license.LicenseModel,
                    IsValid = false,
                    ValidationMessages = new List<string> { "Product mismatch" },
                    BusinessRuleViolations = new List<string> { "License does not match the specified product" },
                    Warnings = new List<string>()
                };
            }

            // Perform enhanced validation if service is available and detailed analysis is requested
            if (_enhancedValidationService != null && includeDetailedAnalysis)
            {
                _logger.LogDebug("Performing enhanced validation with business rules analysis");
                var enhancedResult = await _enhancedValidationService.ValidateWithEnhancedRulesAsync(license);
                
                _logger.LogInformation("Enhanced validation completed. Valid: {IsValid}, Warnings: {WarningCount}, Violations: {ViolationCount}", 
                    enhancedResult.IsValid, enhancedResult.Warnings.Count, enhancedResult.BusinessRuleViolations.Count);
                
                return enhancedResult;
            }
            else
            {
                // Fallback to basic enhanced validation without external service
                _logger.LogDebug("Performing basic enhanced validation without external service");
                return await PerformBasicEnhancedValidationAsync(license);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced license validation for key: {LicenseKey}", licenseKey);
            return new EnhancedLicenseValidationResult
            {
                LicenseKey = licenseKey,
                IsValid = false,
                ValidationMessages = new List<string> { $"Validation error: {ex.Message}" },
                BusinessRuleViolations = new List<string> { "Internal validation error occurred" },
                Warnings = new List<string>()
            };
        }
    }

    /// <summary>
    /// Performs basic enhanced validation when the enhanced validation service is not available
    /// </summary>
    /// <param name="license">License to validate</param>
    /// <returns>Basic enhanced validation result</returns>
    private async Task<EnhancedLicenseValidationResult> PerformBasicEnhancedValidationAsync(ProductLicense license)
    {
        var result = new EnhancedLicenseValidationResult
        {
            LicenseId = license.LicenseId,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseModel,
            IsValid = true,
            ValidationMessages = new List<string>(),
            Warnings = new List<string>(),
            BusinessRuleViolations = new List<string>()
        };

        // Basic validation checks
        if (license.ProductId == Guid.Empty)
            result.BusinessRuleViolations.Add("License must have a valid Product ID");

        if (license.ConsumerId == Guid.Empty)
            result.BusinessRuleViolations.Add("License must have a valid Consumer ID");

        if (string.IsNullOrWhiteSpace(license.LicenseKey))
            result.BusinessRuleViolations.Add("License must have a valid License Key");

        // Date validation
        if (license.ValidFrom >= license.ValidTo)
            result.BusinessRuleViolations.Add("License ValidFrom date must be before ValidTo date");

        // Status and expiration validation
        var now = DateTime.UtcNow;
        var daysUntilExpiry = (license.ValidTo - now).Days;
        result.DaysUntilExpiry = daysUntilExpiry;

        if (license.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Active && license.ValidTo < now)
        {
            result.BusinessRuleViolations.Add("License status is Active but the license has expired");
        }

        if (daysUntilExpiry <= 0)
        {
            result.Warnings.Add("License has expired");
            result.IsExpired = true;
        }
        else if (daysUntilExpiry <= 7)
        {
            result.Warnings.Add($"License expires in {daysUntilExpiry} day(s) - urgent renewal required");
            result.RequiresUrgentRenewal = true;
        }
        else if (daysUntilExpiry <= 30)
        {
            result.Warnings.Add($"License expires in {daysUntilExpiry} day(s) - renewal recommended");
            result.RequiresRenewal = true;
        }

        // License type specific validation
        switch (license.LicenseModel)
        {
            case LicenseType.VolumetricLicense:
                if (!license.MaxAllowedUsers.HasValue || license.MaxAllowedUsers <= 0)
                {
                    result.BusinessRuleViolations.Add("VolumetricLicense must specify MaxAllowedUsers greater than 0");
                }
                result.ValidationMessages.Add($"VolumetricLicense validation completed for {license.MaxAllowedUsers} users");
                break;
            case LicenseType.ProductKey:
                result.ValidationMessages.Add("ProductKey license validation completed");
                break;
            case LicenseType.ProductLicenseFile:
                if (string.IsNullOrWhiteSpace(license.LicenseSignature))
                {
                    result.BusinessRuleViolations.Add("ProductLicenseFile must have a digital signature");
                }
                result.ValidationMessages.Add("ProductLicenseFile license validation completed");
                break;
        }

        // Set final validation status
        result.IsValid = result.BusinessRuleViolations.Count == 0;

        result.ValidationMessages.Add("Basic enhanced validation completed");
        
        _logger.LogInformation("Basic enhanced validation completed for license {LicenseId}. Valid: {IsValid}", 
            license.LicenseId, result.IsValid);

        return await Task.FromResult(result);
    }

    #region TODO: Missing Interface Methods - Require Implementation

    public async Task<ProductLicense> UpdateLicenseAsync(Guid licenseId, LicenseUpdateRequest request, string updatedBy)
    {
        try
        {
            _logger.LogInformation("Updating license {LicenseId} by user {UpdatedBy}", licenseId, updatedBy);

            // Validate request
            var validationResult = await ValidateLicenseUpdateRequestAsync(licenseId, request);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"License update validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Retrieve existing license
                var existingLicense = await _unitOfWork.Licenses.GetByIdAsync(licenseId);
                if (existingLicense == null)
                {
                    throw new ArgumentException($"License with ID {licenseId} not found");
                }

                // Check if license can be updated (not revoked or expired in some cases)
                if (existingLicense.Status == LicenseStatus.Revoked)
                {
                    throw new InvalidOperationException("Cannot update a revoked license");
                }

                // Track original values for audit
                var originalValues = new Dictionary<string, object?>
                {
                    ["ExpiryDate"] = existingLicense.ValidTo,
                    ["MaxUsers"] = existingLicense.MaxAllowedUsers,
                    ["Status"] = existingLicense.Status.ToString(),
                    ["Notes"] = existingLicense.Metadata.ContainsKey("Notes") ? existingLicense.Metadata["Notes"] : null
                };

                // Apply updates
                bool hasChanges = false;

                if (request.ExpiryDate.HasValue && request.ExpiryDate.Value != existingLicense.ValidTo)
                {
                    existingLicense.ValidTo = request.ExpiryDate.Value;
                    hasChanges = true;
                    _logger.LogInformation("Updated license {LicenseId} expiry date to {ExpiryDate}", licenseId, request.ExpiryDate.Value);
                }

                if (request.MaxUsers.HasValue && request.MaxUsers.Value != existingLicense.MaxAllowedUsers)
                {
                    existingLicense.MaxAllowedUsers = request.MaxUsers.Value;
                    hasChanges = true;
                    _logger.LogInformation("Updated license {LicenseId} max users to {MaxUsers}", licenseId, request.MaxUsers.Value);
                }

                // Update metadata if provided
                if (request.Metadata != null)
                {
                    foreach (var kvp in request.Metadata)
                    {
                        if (existingLicense.Metadata.ContainsKey(kvp.Key))
                        {
                            if (!existingLicense.Metadata[kvp.Key].Equals(kvp.Value))
                            {
                                existingLicense.Metadata[kvp.Key] = kvp.Value;
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            existingLicense.Metadata[kvp.Key] = kvp.Value;
                            hasChanges = true;
                        }
                    }
                }

                // Update custom properties if provided
                if (request.CustomProperties != null)
                {
                    // Store custom properties in metadata with a prefix
                    foreach (var kvp in request.CustomProperties)
                    {
                        var key = $"Custom_{kvp.Key}";
                        if (existingLicense.Metadata.ContainsKey(key))
                        {
                            if (!existingLicense.Metadata[key].Equals(kvp.Value))
                            {
                                existingLicense.Metadata[key] = kvp.Value;
                                hasChanges = true;
                            }
                        }
                        else
                        {
                            existingLicense.Metadata[key] = kvp.Value;
                            hasChanges = true;
                        }
                    }
                }

                // Update notes if provided
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    var currentNotes = existingLicense.Metadata.ContainsKey("Notes") ? existingLicense.Metadata["Notes"]?.ToString() : string.Empty;
                    if (currentNotes != request.Notes)
                    {
                        existingLicense.Metadata["Notes"] = request.Notes;
                        hasChanges = true;
                    }
                }

                if (!hasChanges)
                {
                    _logger.LogInformation("No changes detected for license {LicenseId}", licenseId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return existingLicense;
                }

                // Update audit fields
                existingLicense.UpdatedBy = updatedBy;
                existingLicense.UpdatedOn = DateTime.UtcNow;

                // Save the updated license
                await _unitOfWork.Licenses.UpdateAsync(existingLicense.Id, existingLicense);

                // Create audit entry
                var auditEntry = new AuditEntry
                {
                    EntryId = Guid.NewGuid(),
                    EntityType = nameof(ProductLicense),
                    EntityId = licenseId.ToString(),
                    ActionType = "Update",
                    UserName = updatedBy,
                    Timestamp = DateTime.UtcNow,
                    Reason = request.Notes ?? "License update",
                    Metadata = new Dictionary<string, string>
                    {
                        ["OriginalValues"] = JsonSerializer.Serialize(originalValues),
                        ["NewValues"] = JsonSerializer.Serialize(new Dictionary<string, object?>
                        {
                            ["ExpiryDate"] = existingLicense.ValidTo,
                            ["MaxUsers"] = existingLicense.MaxAllowedUsers,
                            ["Status"] = existingLicense.Status.ToString(),
                            ["Notes"] = existingLicense.Metadata.ContainsKey("Notes") ? existingLicense.Metadata["Notes"] : null
                        }),
                        ["UpdateReason"] = request.Notes ?? "License update"
                    }
                };

                await _unitOfWork.AuditEntries.AddAsync(auditEntry);

                // Save all changes
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated license {LicenseId} by user {UpdatedBy}", licenseId, updatedBy);
                return existingLicense;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating license {LicenseId} by user {UpdatedBy}", licenseId, updatedBy);
            throw;
        }
    }

    public async Task<ProductLicense> RegenerateLicenseKeyAsync(Guid licenseId, string regeneratedBy, string reason)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (string.IsNullOrWhiteSpace(regeneratedBy))
            throw new ArgumentException("RegeneratedBy cannot be null or empty", nameof(regeneratedBy));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty for key regeneration", nameof(reason));

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            _logger.LogInformation("Regenerating license key for license {LicenseId} by {RegeneratedBy} with reason: {Reason}", 
                licenseId, regeneratedBy, reason);

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for key regeneration", licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new ArgumentException($"License with ID {licenseId} not found");
            }

            // Check if license can have its key regenerated
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogWarning("Cannot regenerate key for revoked license {LicenseId}", licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Cannot regenerate key for revoked license {licenseId}");
            }

            // Store original key for audit purposes (but don't log it for security)
            var originalKeyLength = license.LicenseKey?.Length ?? 0;
            var originalKeyPrefix = license.LicenseKey?.Length > 8 ? license.LicenseKey.Substring(0, 8) + "..." : "N/A";

            // Generate new license key using the existing generator
            string newLicenseKey;
            try
            {
                var generationRequest = new LicenseGenerationRequest
                {
                    ProductId = license.ProductId,
                    ConsumerId = license.ConsumerId,
                    TierId = license.ProductTierId,
                    ProductName = license.LicenseConsumer?.Product?.Name,
                    ConsumerName = license.LicenseConsumer?.Consumer?.CompanyName,
                    LicenseCode = license.LicenseCode,
                    ProductTier = license.LicenseConsumer?.ProductTier?.Name,
                    LicenseModel = license.LicenseModel,
                    ValidFrom = license.ValidFrom,
                    ExpiryDate = license.ValidTo,
                    MaxUsers = license.MaxAllowedUsers,
                    Notes = "Key regeneration",
                    Metadata = license.Metadata
                };

                var regeneratedLicense = await GenerateLicenseAsync(generationRequest, regeneratedBy);
                newLicenseKey = regeneratedLicense.LicenseKey;

                _logger.LogInformation("Generated new license key for license {LicenseId}", licenseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate new license key for license {LicenseId}", licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                throw new InvalidOperationException($"Failed to generate new license key: {ex.Message}", ex);
            }

            // Update license with new key and regeneration metadata
            var metadata = license.Metadata ?? new Dictionary<string, object>();
            
            // Track key regeneration history
            var regenerationHistory = new List<object>();
            if (metadata.ContainsKey("KeyRegenerationHistory"))
            {
                try
                {
                    var existingHistory = JsonSerializer.Deserialize<List<object>>(metadata["KeyRegenerationHistory"].ToString() ?? "[]");
                    if (existingHistory != null)
                        regenerationHistory = existingHistory;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse existing key regeneration history for license {LicenseId}", licenseId);
                }
            }

            // Add current regeneration to history (without storing actual keys for security)
            regenerationHistory.Add(new
            {
                RegenerationDate = DateTime.UtcNow,
                RegeneratedBy = regeneratedBy,
                Reason = reason,
                PreviousKeyLength = originalKeyLength,
                NewKeyLength = newLicenseKey.Length,
                RegenerationType = "Manual"
            });

            metadata["KeyRegenerationHistory"] = JsonSerializer.Serialize(regenerationHistory);
            metadata["LastKeyRegeneration"] = DateTime.UtcNow;
            metadata["LastKeyRegeneratedBy"] = regeneratedBy;
            metadata["KeyRegenerationReason"] = reason;
            metadata["KeyRegenerationCount"] = regenerationHistory.Count;

            // Update license with new key
            license.LicenseKey = newLicenseKey;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = regeneratedBy;
            license.Metadata = metadata;

            // If the license was previously expired but is still within a reasonable time frame,
            // consider reactivating it (business decision)
            if (license.Status == LicenseStatus.Expired && license.ValidTo > DateTime.UtcNow.AddDays(-30))
            {
                license.Status = LicenseStatus.Active;
                metadata["StatusChangedDuringKeyRegeneration"] = "Reactivated expired license during key regeneration";
                _logger.LogInformation("License {LicenseId} status changed from Expired to Active during key regeneration", licenseId);
            }

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);

            // Create audit entry for key regeneration
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(ProductLicense),
                EntityId = licenseId.ToString(),
                ActionType = "RegenerateKey",
                UserName = regeneratedBy,
                Timestamp = DateTime.UtcNow,
                Reason = reason,
                Metadata = new Dictionary<string, string>
                {
                    ["RegenerationType"] = "Manual",
                    ["PreviousKeyPrefix"] = originalKeyPrefix,
                    ["NewKeyLength"] = newLicenseKey.Length.ToString(),
                    ["RegenerationCount"] = regenerationHistory.Count.ToString(),
                    ["LicenseStatus"] = license.Status.ToString()
                }
            };

            await _unitOfWork.AuditEntries.AddAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully regenerated license key for license {LicenseId} by {RegeneratedBy}. New key length: {KeyLength}. Status: {Status}", 
                licenseId, regeneratedBy, newLicenseKey.Length, license.Status);
            return license;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error regenerating license key for license {LicenseId} by {RegeneratedBy}", 
                licenseId, regeneratedBy);
            throw;
        }
    }

    public async Task<ProductLicense?> GetLicenseByIdAsync(Guid licenseId)
    {
        var response = await _unitOfWork.Licenses.GetByIdWithAllIncludesAsync(licenseId);
        if (response == null)
        {
            _logger.LogWarning("License with ID {LicenseId} not found", licenseId);
            return null;
        }
        return response;
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByConsumerAsync(Guid consumerId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        var response = await _unitOfWork.Licenses.GetByConsumerIdAsync(consumerId);
        if (response == null || !response.Any())
        {
            return Enumerable.Empty<ProductLicense>();
        } 

        return response
            .Where(l => !status.HasValue || l.Status == status.Value)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByProductAsync(Guid productId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>
                {
                    { nameof(ProductLicense.ProductId), productId }
                },
                Page = pageNumber,
                PageSize = pageSize
            };

            if (status.HasValue)
            {
                searchRequest.Filters.Add("Status", status.Value.ToString());
            }

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            return result.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting licenses for product: {ProductId}", productId);
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysAhead = 30)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysAhead);
            
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>
                {
                    { "Status", LicenseStatus.Active.ToString() }
                },
                Page = 1,
                PageSize = 1000 // Get a large number of expiring licenses
            };

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            
            // Filter by expiry date in memory since complex date filtering might not be supported by search
            return result.Results.Where(l => 
                l.ValidTo <= expiryDate && 
                l.ValidTo > DateTime.UtcNow && 
                l.Status == LicenseStatus.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring licenses for {DaysAhead} days", daysAhead);
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiredLicensesAsync()
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>(),
                Page = 1,
                PageSize = 1000 // Get a large number of licenses
            };

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            
            // Filter expired licenses in memory
            return result.Results.Where(l => 
                l.ValidTo <= DateTime.UtcNow && 
                (l.Status == LicenseStatus.Active || l.Status == LicenseStatus.Expired));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired licenses");
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<bool> ActivateLicenseAsync(Guid licenseId, ActivationInfo activationInfo)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (activationInfo == null)
            throw new ArgumentNullException(nameof(activationInfo));
        if (string.IsNullOrWhiteSpace(activationInfo.ActivatedBy))
            throw new ArgumentException("ActivatedBy cannot be null or empty", nameof(activationInfo.ActivatedBy));

        try
        {
            _logger.LogInformation("Activating license {LicenseId} by {ActivatedBy}", licenseId, activationInfo.ActivatedBy);

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for activation", licenseId);
                return false;
            }

            // Check if license is already active
            if (license.Status == LicenseStatus.Active)
            {
                _logger.LogInformation("License {LicenseId} is already active", licenseId);
                return true;
            }

            // Check if license is in a state that can be activated
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogWarning("Cannot activate revoked license {LicenseId}", licenseId);
                return false;
            }

            // Check if license is expired
            if (license.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Cannot activate expired license {LicenseId} (expired: {ExpiryDate})", 
                    licenseId, license.ValidTo);
                return false;
            }

            // Update license status
            license.Status = LicenseStatus.Active;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = activationInfo.ActivatedBy;

            // Update activation metadata if provided
            if (activationInfo.ActivationMetadata.Any())
            {
                var metadata = license.Metadata ?? new Dictionary<string, object>();

                metadata["LastActivation"] = activationInfo.ActivationDate;
                metadata["ActivatedBy"] = activationInfo.ActivatedBy;
                if (!string.IsNullOrWhiteSpace(activationInfo.MachineId))
                    metadata["LastActivationMachine"] = activationInfo.MachineId;

                // Merge activation metadata
                foreach (var kvp in activationInfo.ActivationMetadata)
                {
                    metadata[kvp.Key] = kvp.Value;
                }

                license.Metadata = metadata;
            }

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully activated license {LicenseId} by {ActivatedBy}", 
                licenseId, activationInfo.ActivatedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating license {LicenseId} by {ActivatedBy}", 
                licenseId, activationInfo.ActivatedBy);
            throw;
        }
    }

    public async Task<bool> DeactivateLicenseAsync(Guid licenseId, string deactivatedBy, string? reason = null)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("DeactivatedBy cannot be null or empty", nameof(deactivatedBy));

        try
        {
            _logger.LogInformation("Deactivating license {LicenseId} by {DeactivatedBy} with reason: {Reason}", 
                licenseId, deactivatedBy, reason ?? "No reason provided");

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for deactivation", licenseId);
                return false;
            }

            // Check if license is already suspended (deactivated)
            if (license.Status == LicenseStatus.Suspended)
            {
                _logger.LogInformation("License {LicenseId} is already deactivated (suspended)", licenseId);
                return true;
            }

            // Check if license is revoked (can't deactivate revoked licenses)
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogWarning("Cannot deactivate revoked license {LicenseId}", licenseId);
                return false;
            }

            // Update license status
            license.Status = LicenseStatus.Suspended;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = deactivatedBy;

            // Update deactivation metadata
            var metadata = license.Metadata ?? new Dictionary<string, object>();
            metadata["LastDeactivation"] = DateTime.UtcNow;
            metadata["DeactivatedBy"] = deactivatedBy;
            if (!string.IsNullOrWhiteSpace(reason))
                metadata["DeactivationReason"] = reason;
            
            license.Metadata = metadata;

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deactivated license {LicenseId} by {DeactivatedBy}", 
                licenseId, deactivatedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating license {LicenseId} by {DeactivatedBy}", 
                licenseId, deactivatedBy);
            throw;
        }
    }

    public async Task<bool> SuspendLicenseAsync(Guid licenseId, string suspendedBy, string reason, DateTime? suspendUntil = null)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (string.IsNullOrWhiteSpace(suspendedBy))
            throw new ArgumentException("SuspendedBy cannot be null or empty", nameof(suspendedBy));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty for suspension", nameof(reason));

        try
        {
            _logger.LogInformation("Suspending license {LicenseId} by {SuspendedBy} with reason: {Reason} until: {SuspendUntil}", 
                licenseId, suspendedBy, reason, suspendUntil?.ToString("yyyy-MM-dd") ?? "indefinite");

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for suspension", licenseId);
                return false;
            }

            // Check if license is already suspended
            if (license.Status == LicenseStatus.Suspended)
            {
                _logger.LogInformation("License {LicenseId} is already suspended", licenseId);
                return true;
            }

            // Check if license is revoked (can't suspend revoked licenses)
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogWarning("Cannot suspend revoked license {LicenseId}", licenseId);
                return false;
            }

            // Check if license is expired
            if (license.Status == LicenseStatus.Expired)
            {
                _logger.LogWarning("Cannot suspend expired license {LicenseId}", licenseId);
                return false;
            }

            // Store original status for potential restoration
            var metadata = license.Metadata ?? new Dictionary<string, object>();
            metadata["OriginalStatusBeforeSuspension"] = license.Status.ToString();
            metadata["SuspensionDate"] = DateTime.UtcNow;
            metadata["SuspendedBy"] = suspendedBy;
            metadata["SuspensionReason"] = reason;
            
            if (suspendUntil.HasValue)
            {
                metadata["SuspendedUntil"] = suspendUntil.Value;
                _logger.LogInformation("License {LicenseId} will be suspended until {SuspendUntil}", 
                    licenseId, suspendUntil.Value);
            }
            else
            {
                metadata["SuspendedUntil"] = "Indefinite";
                _logger.LogInformation("License {LicenseId} will be suspended indefinitely", licenseId);
            }

            // Update license status
            license.Status = LicenseStatus.Suspended;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = suspendedBy;
            license.Metadata = metadata;

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully suspended license {LicenseId} by {SuspendedBy}", 
                licenseId, suspendedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending license {LicenseId} by {SuspendedBy}", 
                licenseId, suspendedBy);
            throw;
        }
    }

    public async Task<bool> RevokeLicenseAsync(Guid licenseId, string revokedBy, string reason)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (string.IsNullOrWhiteSpace(revokedBy))
            throw new ArgumentException("RevokedBy cannot be null or empty", nameof(revokedBy));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be null or empty for revocation", nameof(reason));

        try
        {
            _logger.LogInformation("Revoking license {LicenseId} by {RevokedBy} with reason: {Reason}", 
                licenseId, revokedBy, reason);

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for revocation", licenseId);
                return false;
            }

            // Check if license is already revoked
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogInformation("License {LicenseId} is already revoked", licenseId);
                return true;
            }

            // Store original status and revocation details
            var metadata = license.Metadata ?? new Dictionary<string, object>();
            metadata["OriginalStatusBeforeRevocation"] = license.Status.ToString();
            metadata["RevocationDate"] = DateTime.UtcNow;
            metadata["RevokedBy"] = revokedBy;
            metadata["RevocationReason"] = reason;

            // Update license status and revocation fields
            license.Status = LicenseStatus.Revoked;
            license.RevokedAt = DateTime.UtcNow;
            license.RevocationReason = reason;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = revokedBy;
            license.Metadata = metadata;

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully revoked license {LicenseId} by {RevokedBy}. Reason: {Reason}", 
                licenseId, revokedBy, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking license {LicenseId} by {RevokedBy}", 
                licenseId, revokedBy);
            throw;
        }
    }

    public async Task<bool> RenewLicenseAsync(Guid licenseId, DateTime newExpiryDate, string renewedBy)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));
        if (newExpiryDate <= DateTime.UtcNow)
            throw new ArgumentException("New expiry date must be in the future", nameof(newExpiryDate));
        if (string.IsNullOrWhiteSpace(renewedBy))
            throw new ArgumentException("RenewedBy cannot be null or empty", nameof(renewedBy));

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            _logger.LogInformation("Renewing license {LicenseId} with new expiry date {NewExpiryDate} by {RenewedBy}", 
                licenseId, newExpiryDate, renewedBy);

            // Get the license
            var license = await GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found for renewal", licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            // Check if license can be renewed
            if (license.Status == LicenseStatus.Revoked)
            {
                _logger.LogWarning("Cannot renew revoked license {LicenseId}", licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            // Validate new expiry date is reasonable
            if (newExpiryDate <= license.ValidFrom)
            {
                _logger.LogWarning("New expiry date {NewExpiryDate} must be after license valid from date {ValidFrom} for license {LicenseId}", 
                    newExpiryDate, license.ValidFrom, licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            // Don't allow renewal more than 10 years from now (business rule)
            if (newExpiryDate > DateTime.UtcNow.AddYears(10))
            {
                _logger.LogWarning("New expiry date {NewExpiryDate} exceeds maximum allowed renewal period for license {LicenseId}", 
                    newExpiryDate, licenseId);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }

            // Store original values for audit
            var originalExpiryDate = license.ValidTo;
            var originalStatus = license.Status;

            // Update license renewal information
            var metadata = license.Metadata ?? new Dictionary<string, object>();
            
            // Track renewal history
            var renewalHistory = new List<object>();
            if (metadata.ContainsKey("RenewalHistory"))
            {
                try
                {
                    var existingHistory = JsonSerializer.Deserialize<List<object>>(metadata["RenewalHistory"].ToString() ?? "[]");
                    if (existingHistory != null)
                        renewalHistory = existingHistory;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse existing renewal history for license {LicenseId}", licenseId);
                }
            }

            // Add current renewal to history
            renewalHistory.Add(new
            {
                RenewalDate = DateTime.UtcNow,
                PreviousExpiryDate = originalExpiryDate,
                NewExpiryDate = newExpiryDate,
                RenewedBy = renewedBy,
                RenewalType = license.Status == LicenseStatus.Expired ? "Post-Expiry" : "Pre-Expiry"
            });

            metadata["RenewalHistory"] = JsonSerializer.Serialize(renewalHistory);
            metadata["LastRenewalDate"] = DateTime.UtcNow;
            metadata["RenewedBy"] = renewedBy;
            metadata["PreviousExpiryDate"] = originalExpiryDate;
            metadata["RenewalCount"] = renewalHistory.Count;

            // Update license fields
            license.ValidTo = newExpiryDate;
            license.UpdatedOn = DateTime.UtcNow;
            license.UpdatedBy = renewedBy;
            license.Metadata = metadata;

            // Update status based on new expiry date
            if (license.Status == LicenseStatus.Expired && newExpiryDate > DateTime.UtcNow)
            {
                // Reactivate expired license if renewed with future date
                license.Status = LicenseStatus.Active;
                metadata["StatusChangedDuringRenewal"] = $"Changed from {originalStatus} to {license.Status}";
                _logger.LogInformation("License {LicenseId} status changed from Expired to Active during renewal", licenseId);
            }
            else if (license.Status == LicenseStatus.Suspended)
            {
                // Optionally reactivate suspended licenses during renewal (business decision)
                license.Status = LicenseStatus.Active;
                metadata["StatusChangedDuringRenewal"] = $"Changed from {originalStatus} to {license.Status}";
                metadata["SuspensionClearedDuringRenewal"] = DateTime.UtcNow;
                _logger.LogInformation("License {LicenseId} status changed from Suspended to Active during renewal", licenseId);
            }

            // Save changes
            await _unitOfWork.Licenses.UpdateAsync(license.LicenseId, license);

            // Create audit entry for renewal
            var auditEntry = new AuditEntry
            {
                EntryId = Guid.NewGuid(),
                EntityType = nameof(ProductLicense),
                EntityId = licenseId.ToString(),
                ActionType = "Renew",
                UserName = renewedBy,
                Timestamp = DateTime.UtcNow,
                Reason = "License renewal",
                Metadata = new Dictionary<string, string>
                {
                    ["PreviousExpiryDate"] = originalExpiryDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["NewExpiryDate"] = newExpiryDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["RenewalType"] = originalStatus == LicenseStatus.Expired ? "Post-Expiry" : "Pre-Expiry",
                    ["StatusBefore"] = originalStatus.ToString(),
                    ["StatusAfter"] = license.Status.ToString(),
                    ["RenewalCount"] = renewalHistory.Count.ToString()
                }
            };

            await _unitOfWork.AuditEntries.AddAsync(auditEntry);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully renewed license {LicenseId} from {OriginalExpiry} to {NewExpiry} by {RenewedBy}. Status: {Status}", 
                licenseId, originalExpiryDate, newExpiryDate, renewedBy, license.Status);
            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error renewing license {LicenseId} by {RenewedBy}", licenseId, renewedBy);
            throw;
        }
    }

    public async Task<bool> UpdateLicenseStatusAsync(Guid licenseId, LicenseStatus status, string updatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("UpdateLicenseStatusAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeleteLicenseAsync(Guid licenseId, string deletedBy)
    {
        // TODO: Implement
        _logger.LogWarning("DeleteLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> LicenseExistsAsync(string licenseKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return false;

            var license = await GetLicenseByKeyAsync(licenseKey);
            return license != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if license exists: {LicenseKey}", licenseKey);
            return false;
        }
    }

    public Task<ValidationResult> ValidateLicenseDataAsync(ProductLicense license)
    {
        try
        {
            var errors = new List<string>();

            if (license == null)
            {
                errors.Add("License cannot be null");
                return Task.FromResult(ValidationResult.Failure(errors.ToArray()));
            }

            if (license.ProductId == Guid.Empty)
                errors.Add("License must have a valid ProductId");

            if (license.ConsumerId == Guid.Empty)
                errors.Add("License must have a valid ConsumerId");

            if (string.IsNullOrWhiteSpace(license.LicenseKey))
                errors.Add("License must have a valid LicenseKey");

            if (license.ValidFrom >= license.ValidTo)
                errors.Add("ValidFrom must be before ValidTo");

            return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license data");
            return Task.FromResult(ValidationResult.Failure("An error occurred during license data validation"));
        }
    }

    public Task<ValidationResult> ValidateLicenseGenerationRequestAsync(LicenseGenerationRequest request)
    {
        try
        {
            var errors = new List<string>();

            // Basic validation
            if (request == null)
            {
                errors.Add("License generation request cannot be null");
                return Task.FromResult(ValidationResult.Failure(errors.ToArray()));
            }

            if (request.ProductId == Guid.Empty)
                errors.Add("ProductId is required");

            if (request.ConsumerId == Guid.Empty)
                errors.Add("ConsumerId is required");

            if (request.ExpiryDate.HasValue && request.ExpiryDate <= DateTime.UtcNow)
                errors.Add("Expiry date must be in the future");

            if (request.MaxUsers.HasValue && request.MaxUsers <= 0)
                errors.Add("Max users must be greater than 0");

            if (request.MaxDevices.HasValue && request.MaxDevices <= 0)
                errors.Add("Max devices must be greater than 0");

            return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license generation request");
            return Task.FromResult(ValidationResult.Failure("An error occurred during validation"));
        }
    }

    public async Task<ValidationResult> ValidateLicenseUpdateRequestAsync(Guid licenseId, LicenseUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Validating license update request for license {LicenseId}", licenseId);

            var errors = new List<string>();

            // Basic request validation
            if (request == null)
            {
                errors.Add("Update request cannot be null");
                return ValidationResult.Failure(errors.ToArray());
            }

            // Validate license exists
            var existingLicense = await _unitOfWork.Licenses.GetByIdAsync(licenseId);
            if (existingLicense == null)
            {
                errors.Add($"License with ID {licenseId} not found");
                return ValidationResult.Failure(errors.ToArray());
            }

            // Validate expiry date if provided
            if (request.ExpiryDate.HasValue)
            {
                if (request.ExpiryDate.Value <= DateTime.UtcNow)
                {
                    errors.Add("Expiry date must be in the future");
                }

                if (request.ExpiryDate.Value <= existingLicense.ValidFrom)
                {
                    errors.Add("Expiry date must be after the license valid from date");
                }

                // Don't allow extending expired licenses beyond reasonable limits
                if (existingLicense.IsExpired && request.ExpiryDate.Value > DateTime.UtcNow.AddYears(10))
                {
                    errors.Add("Cannot extend expired license more than 10 years from current date");
                }
            }

            // Validate max users if provided
            if (request.MaxUsers.HasValue)
            {
                if (request.MaxUsers.Value < 0)
                {
                    errors.Add("Maximum users cannot be negative");
                }

                if (request.MaxUsers.Value > 1000000) // Reasonable upper limit
                {
                    errors.Add("Maximum users cannot exceed 1,000,000");
                }
            }

            // Validate max devices if provided
            if (request.MaxDevices.HasValue)
            {
                if (request.MaxDevices.Value < 0)
                {
                    errors.Add("Maximum devices cannot be negative");
                }

                if (request.MaxDevices.Value > 1000000) // Reasonable upper limit
                {
                    errors.Add("Maximum devices cannot exceed 1,000,000");
                }
            }

            // Validate notes length
            if (!string.IsNullOrEmpty(request.Notes) && request.Notes.Length > 2000)
            {
                errors.Add("Notes cannot exceed 2000 characters");
            }

            // Validate metadata if provided
            if (request.Metadata != null)
            {
                if (request.Metadata.Count > 50)
                {
                    errors.Add("Metadata cannot have more than 50 entries");
                }

                foreach (var kvp in request.Metadata)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                    {
                        errors.Add("Metadata keys cannot be null or empty");
                        break;
                    }

                    if (kvp.Key.Length > 100)
                    {
                        errors.Add("Metadata keys cannot exceed 100 characters");
                        break;
                    }

                    var valueString = kvp.Value?.ToString() ?? "";
                    if (valueString.Length > 1000)
                    {
                        errors.Add("Metadata values cannot exceed 1000 characters");
                        break;
                    }
                }
            }

            // Validate custom properties if provided
            if (request.CustomProperties != null)
            {
                if (request.CustomProperties.Count > 50)
                {
                    errors.Add("Custom properties cannot have more than 50 entries");
                }

                foreach (var kvp in request.CustomProperties)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                    {
                        errors.Add("Custom property keys cannot be null or empty");
                        break;
                    }

                    if (kvp.Key.Length > 100)
                    {
                        errors.Add("Custom property keys cannot exceed 100 characters");
                        break;
                    }

                    var valueString = kvp.Value?.ToString() ?? "";
                    if (valueString.Length > 1000)
                    {
                        errors.Add("Custom property values cannot exceed 1000 characters");
                        break;
                    }
                }
            }

            _logger.LogInformation("License update validation completed for license {LicenseId}. Errors: {ErrorCount}", 
                licenseId, errors.Count);

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license update request for license {LicenseId}", licenseId);
            return ValidationResult.Failure("An error occurred during validation");
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetAllLicensesAsync(LicenseStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        // Delegate to GetLicensesAsync which has the same functionality
        return await GetLicensesAsync(status, searchTerm, pageNumber, pageSize);
    }

    public async Task<int> GetLicenseCountAsync(LicenseStatus? status = null, string? searchTerm = null)
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>(),
                Page = 1,
                PageSize = int.MaxValue, // Get all to count
                Query = searchTerm
            };

            if (status.HasValue)
            {
                searchRequest.Filters.Add("Status", status.Value.ToString());
            }

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            return result.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license count");
            return 0;
        }
    }

    public async Task<LicenseUsageStatistics> GetLicenseUsageStatisticsAsync(Guid? productId = null, Guid? consumerId = null)
    {
        var result = await _unitOfWork.Licenses.GetUsageStatisticsAsync(productId, consumerId);
        _logger.LogWarning("GetLicenseUsageStatisticsAsync not implemented");
        return result;
    }

    public async Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(Guid licenseId)
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

    public async Task<IEnumerable<ProductLicense>> GetLicensesAsync(LicenseStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
        {
            Filters = new Dictionary<string, object>(),
            Page = pageNumber,
            PageSize = pageSize,
            Query= searchTerm
        };

        if (status.HasValue)
        {
            searchRequest.Filters.Add("Status", status.Value.ToString());
        }



        var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
        return result.Results;
    }
 

    #endregion
}
