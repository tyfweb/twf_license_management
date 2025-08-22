using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Enhanced license validation service that provides additional validation logic
/// beyond the basic validation in ProductLicenseService
/// </summary>
public class LicenseValidationEnhancementService
{
    private readonly ILogger<LicenseValidationEnhancementService> _logger;
    private readonly IProductLicenseRepository _licenseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IConsumerAccountRepository _consumerRepository;

    public LicenseValidationEnhancementService(
        ILogger<LicenseValidationEnhancementService> logger,
        IProductLicenseRepository licenseRepository,
        IProductRepository productRepository,
        IConsumerAccountRepository consumerRepository)
    {
        _logger = logger;
        _licenseRepository = licenseRepository;
        _productRepository = productRepository;
        _consumerRepository = consumerRepository;
    }

    /// <summary>
    /// Performs enhanced validation of a license with business rules and usage limits
    /// </summary>
    /// <param name="license">The license to validate</param>
    /// <returns>Enhanced validation result with detailed information</returns>
    public async Task<EnhancedLicenseValidationResult> ValidateWithEnhancedRulesAsync(ProductLicense license)
    {
        try
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
            await ValidateBasicLicensePropertiesAsync(license, result);

            // License type-specific validation
            await ValidateLicenseTypeSpecificRulesAsync(license, result);

            // Business rules validation
            await ValidateBusinessRulesAsync(license, result);

            // Usage and activation tracking
            await ValidateUsageAndActivationAsync(license, result);

            // Expiration and renewal validation
            await ValidateExpirationAndRenewalAsync(license, result);

            // Set final validation status
            result.IsValid = result.BusinessRuleViolations.Count == 0 && 
                           result.ValidationMessages.All(m => !m.Contains("ERROR", StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation("Enhanced validation completed for license {LicenseId}. Valid: {IsValid}, Warnings: {WarningCount}", 
                license.LicenseId, result.IsValid, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced license validation for license {LicenseId}", license.LicenseId);
            return new EnhancedLicenseValidationResult
            {
                LicenseId = license.LicenseId,
                LicenseKey = license.LicenseKey,
                LicenseType = license.LicenseModel,
                IsValid = false,
                ValidationMessages = new List<string> { $"Validation error: {ex.Message}" },
                Warnings = new List<string>(),
                BusinessRuleViolations = new List<string> { "Internal validation error occurred" }
            };
        }
    }

    /// <summary>
    /// Validates basic license properties and structure
    /// </summary>
    private async Task ValidateBasicLicensePropertiesAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // Validate required fields
        if (license.ProductId == Guid.Empty)
            result.BusinessRuleViolations.Add("License must have a valid Product ID");

        if (license.ConsumerId == Guid.Empty)
            result.BusinessRuleViolations.Add("License must have a valid Consumer ID");

        if (string.IsNullOrWhiteSpace(license.LicenseKey))
            result.BusinessRuleViolations.Add("License must have a valid License Key");

        // Validate dates
        if (license.ValidFrom >= license.ValidTo)
            result.BusinessRuleViolations.Add("License ValidFrom date must be before ValidTo date");

        // Check if product exists
        var product = await _productRepository.GetByIdAsync(license.ProductId);
        if (product == null)
        {
            result.BusinessRuleViolations.Add($"Product with ID {license.ProductId} does not exist");
        }
        else
        {
            result.ProductName = product.Name;
            result.ProductVersion = product.Version?.ToString() ?? "Unknown";
        }

        // Check if consumer exists
        var consumer = await _consumerRepository.GetByIdAsync(license.ConsumerId);
        if (consumer == null)
        {
            result.BusinessRuleViolations.Add($"Consumer with ID {license.ConsumerId} does not exist");
        }
        else
        {
            result.ConsumerName = consumer.CompanyName ?? consumer.PrimaryContact?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Validates license type-specific rules
    /// </summary>
    private async Task ValidateLicenseTypeSpecificRulesAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        switch (license.LicenseModel)
        {
            case LicenseType.ProductKey:
                await ValidateProductKeyLicenseAsync(license, result);
                break;
            case LicenseType.ProductLicenseFile:
                await ValidateProductLicenseFileAsync(license, result);
                break;
            case LicenseType.VolumetricLicense:
                await ValidateVolumetricLicenseAsync(license, result);
                break;
            default:
                result.Warnings.Add($"Unknown license type: {license.LicenseModel}");
                break;
        }
    }

    /// <summary>
    /// Validates ProductKey specific rules
    /// </summary>
    private async Task ValidateProductKeyLicenseAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // Validate key format (XXXX-XXXX-XXXX-XXXX)
        if (!IsValidProductKeyFormat(license.LicenseKey))
        {
            result.BusinessRuleViolations.Add("ProductKey license must follow XXXX-XXXX-XXXX-XXXX format");
        }

        // ProductKey licenses should not have MaxAllowedUsers (single user)
        if (license.MaxAllowedUsers.HasValue && license.MaxAllowedUsers > 1)
        {
            result.Warnings.Add("ProductKey licenses are typically single-user. Consider using VolumetricLicense for multi-user scenarios");
        }

        result.ValidationMessages.Add("ProductKey license validation completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates ProductLicenseFile specific rules
    /// </summary>
    private async Task ValidateProductLicenseFileAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // ProductLicenseFile should have digital signature (using LicenseSignature)
        if (string.IsNullOrWhiteSpace(license.LicenseSignature))
        {
            result.BusinessRuleViolations.Add("ProductLicenseFile must have a digital signature for offline validation");
        }

        // Check if license key exists for integrity
        if (string.IsNullOrWhiteSpace(license.LicenseKey))
        {
            result.BusinessRuleViolations.Add("ProductLicenseFile must have a license key for integrity verification");
        }

        result.ValidationMessages.Add("ProductLicenseFile license validation completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates VolumetricLicense specific rules
    /// </summary>
    private async Task ValidateVolumetricLicenseAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // VolumetricLicense must have MaxAllowedUsers
        if (!license.MaxAllowedUsers.HasValue || license.MaxAllowedUsers <= 0)
        {
            result.BusinessRuleViolations.Add("VolumetricLicense must specify MaxAllowedUsers greater than 0");
        }

        // Validate key format (XXXX-XXXX-XXXX-NNNN where NNNN is user count)
        if (!IsValidVolumetricKeyFormat(license.LicenseKey))
        {
            result.BusinessRuleViolations.Add("VolumetricLicense must follow XXXX-XXXX-XXXX-NNNN format where NNNN represents user count");
        }

        // Check for reasonable user limits
        if (license.MaxAllowedUsers > 10000)
        {
            result.Warnings.Add("VolumetricLicense has very high user limit. Verify this is intentional");
        }

        result.ValidationMessages.Add($"VolumetricLicense validation completed for {license.MaxAllowedUsers} users");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates business rules and policies
    /// </summary>
    private async Task ValidateBusinessRulesAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // Check for duplicate active licenses for same product/consumer using available methods
        var existingLicenses = await _licenseRepository.GetByConsumerIdAsync(license.ConsumerId);
        var activeDuplicates = existingLicenses.Where(l => 
            l.LicenseId != license.LicenseId && 
            l.ProductId == license.ProductId &&
            l.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Active).ToList();

        if (activeDuplicates.Any())
        {
            result.Warnings.Add($"Consumer already has {activeDuplicates.Count} active license(s) for this product");
        }

        // Validate license status consistency
        var now = DateTime.UtcNow;
        if (license.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Active && license.ValidTo < now)
        {
            result.BusinessRuleViolations.Add("License status is Active but the license has expired");
        }

        if (license.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Expired && license.ValidTo >= now)
        {
            result.BusinessRuleViolations.Add("License status is Expired but the license is still valid");
        }

        result.ValidationMessages.Add("Business rules validation completed");
    }

    /// <summary>
    /// Validates usage and activation tracking
    /// </summary>
    private async Task ValidateUsageAndActivationAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        // For VolumetricLicense, check current usage vs limits
        if (license.LicenseModel == LicenseType.VolumetricLicense && license.MaxAllowedUsers.HasValue)
        {
            // TODO: When activation tracking is implemented, add usage validation here
            result.ValidationMessages.Add($"Usage validation: License allows up to {license.MaxAllowedUsers} users");
        }

        // Check activation status for ProductKey licenses
        if (license.LicenseModel == LicenseType.ProductKey)
        {
            // TODO: When activation tracking is implemented, add activation validation here
            result.ValidationMessages.Add("Activation validation: ProductKey license requires online activation");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates expiration and renewal requirements
    /// </summary>
    private Task ValidateExpirationAndRenewalAsync(ProductLicense license, EnhancedLicenseValidationResult result)
    {
        var now = DateTime.UtcNow;
        var daysUntilExpiry = (license.ValidTo - now).Days;

        result.DaysUntilExpiry = daysUntilExpiry;

        // Expiration warnings
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

        // Check for licenses that have been expired for too long
        if (daysUntilExpiry < -90)
        {
            result.BusinessRuleViolations.Add("License has been expired for more than 90 days - consider revocation");
        }

        result.ValidationMessages.Add($"Expiration validation completed: {daysUntilExpiry} days until expiry");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates ProductKey format (XXXX-XXXX-XXXX-XXXX)
    /// </summary>
    private static bool IsValidProductKeyFormat(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        var parts = licenseKey.Split('-');
        return parts.Length == 4 && parts.All(p => p.Length == 4 && p.All(char.IsLetterOrDigit));
    }

    /// <summary>
    /// Validates VolumetricLicense format (XXXX-XXXX-XXXX-NNNN)
    /// </summary>
    private static bool IsValidVolumetricKeyFormat(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        var parts = licenseKey.Split('-');
        if (parts.Length != 4)
            return false;

        // First three parts should be alphanumeric
        for (int i = 0; i < 3; i++)
        {
            if (parts[i].Length != 4 || !parts[i].All(char.IsLetterOrDigit))
                return false;
        }

        // Last part should be numeric (user count)
        return parts[3].Length == 4 && parts[3].All(char.IsDigit) && int.TryParse(parts[3], out var userCount) && userCount > 0;
    }
}

/// <summary>
/// Enhanced license validation result with detailed information
/// </summary>
public class EnhancedLicenseValidationResult
{
    public Guid LicenseId { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public bool RequiresRenewal { get; set; }
    public bool RequiresUrgentRenewal { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductVersion { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public List<string> ValidationMessages { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> BusinessRuleViolations { get; set; } = new();

    /// <summary>
    /// Gets a summary of the validation result
    /// </summary>
    public string GetValidationSummary()
    {
        var summary = IsValid ? "Valid" : "Invalid";
        if (Warnings.Count > 0)
            summary += $" with {Warnings.Count} warning(s)";
        if (BusinessRuleViolations.Count > 0)
            summary += $" and {BusinessRuleViolations.Count} violation(s)";
        return summary;
    }
}
