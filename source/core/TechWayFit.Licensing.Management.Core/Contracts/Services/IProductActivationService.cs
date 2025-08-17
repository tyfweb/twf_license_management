using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for managing product key activations
/// </summary>
public interface IProductActivationService
{
    /// <summary>
    /// Creates a new product key registration with pending activation status
    /// </summary>
    /// <param name="request">Product key creation request</param>
    /// <param name="createdBy">User creating the product key</param>
    /// <returns>Product key registration result</returns>
    Task<ProductKeyRegistrationResult> CreateProductKeyAsync(CreateProductKeyRequest request, string createdBy);

    /// <summary>
    /// Activates a product key with machine details
    /// </summary>
    /// <param name="request">Activation request with product key and machine details</param>
    /// <returns>Activation result with signature</returns>
    Task<ProductActivationResult> ActivateProductKeyAsync(ProductActivationRequest request);

    /// <summary>
    /// Retrieves activation details using signature
    /// </summary>
    /// <param name="signature">Activation signature</param>
    /// <returns>Activation details</returns>
    Task<ProductActivationDetails?> GetActivationBySignatureAsync(string signature);

    /// <summary>
    /// Validates if a product key exists and can be activated
    /// </summary>
    /// <param name="productKey">Product key to validate</param>
    /// <returns>Validation result</returns>
    Task<ProductKeyValidationResult> ValidateProductKeyAsync(string productKey);

    /// <summary>
    /// Deactivates a product key activation
    /// </summary>
    /// <param name="signature">Activation signature</param>
    /// <param name="deactivatedBy">User deactivating</param>
    /// <param name="reason">Deactivation reason</param>
    /// <returns>True if deactivated successfully</returns>
    Task<bool> DeactivateProductKeyAsync(string signature, string deactivatedBy, string? reason = null);

    /// <summary>
    /// Gets all activations for a product key
    /// </summary>
    /// <param name="productKey">Product key</param>
    /// <returns>List of activations</returns>
    Task<IEnumerable<ProductActivationDetails>> GetActivationsByProductKeyAsync(string productKey);
}

/// <summary>
/// Request for creating a new product key
/// </summary>
public class CreateProductKeyRequest
{
    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Consumer ID
    /// </summary>
    public Guid ConsumerId { get; set; }

    /// <summary>
    /// Product tier ID (optional)
    /// </summary>
    public Guid? ProductTierId { get; set; }

    /// <summary>
    /// License validity start date
    /// </summary>
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// License validity end date
    /// </summary>
    public DateTime ValidTo { get; set; }

    /// <summary>
    /// Maximum number of activations allowed
    /// </summary>
    public int MaxActivations { get; set; } = 1;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request for activating a product key
/// </summary>
public class ProductActivationRequest
{
    /// <summary>
    /// Product key to activate
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Unique machine identifier
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Machine name
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Hardware fingerprint
    /// </summary>
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// IP address of activation request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Additional activation data
    /// </summary>
    public Dictionary<string, object>? ActivationData { get; set; }
}

/// <summary>
/// Result of product key registration
/// </summary>
public class ProductKeyRegistrationResult
{
    /// <summary>
    /// Generated product key
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// License ID
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Registration status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Maximum activations allowed
    /// </summary>
    public int MaxActivations { get; set; }

    /// <summary>
    /// License validity period
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// License expiry date
    /// </summary>
    public DateTime ValidTo { get; set; }

    public static ProductKeyRegistrationResult CreateSuccess(string productKey, Guid licenseId, int maxActivations, DateTime validFrom, DateTime validTo)
    {
        return new ProductKeyRegistrationResult
        {
            ProductKey = productKey,
            LicenseId = licenseId,
            MaxActivations = maxActivations,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Success = true
        };
    }

    public static ProductKeyRegistrationResult CreateFailure(string errorMessage)
    {
        return new ProductKeyRegistrationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of product activation
/// </summary>
public class ProductActivationResult
{
    /// <summary>
    /// Activation success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Unique activation signature
    /// </summary>
    public string? ActivationSignature { get; set; }

    /// <summary>
    /// Activation ID
    /// </summary>
    public Guid? ActivationId { get; set; }

    /// <summary>
    /// Activation end date
    /// </summary>
    public DateTime? ActivationEndDate { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// License details
    /// </summary>
    public ProductActivationDetails? ActivationDetails { get; set; }

    public static ProductActivationResult CreateSuccess(string signature, Guid activationId, DateTime endDate, ProductActivationDetails details)
    {
        return new ProductActivationResult
        {
            Success = true,
            ActivationSignature = signature,
            ActivationId = activationId,
            ActivationEndDate = endDate,
            ActivationDetails = details
        };
    }

    public static ProductActivationResult CreateFailure(string errorMessage)
    {
        return new ProductActivationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Product activation details
/// </summary>
public class ProductActivationDetails
{
    /// <summary>
    /// Activation ID
    /// </summary>
    public Guid ActivationId { get; set; }

    /// <summary>
    /// License ID
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Product key
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Machine ID
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Machine name
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Activation date
    /// </summary>
    public DateTime ActivationDate { get; set; }

    /// <summary>
    /// Activation end date
    /// </summary>
    public DateTime? ActivationEndDate { get; set; }

    /// <summary>
    /// Activation status
    /// </summary>
    public ProductActivationStatus Status { get; set; }

    /// <summary>
    /// Activation signature
    /// </summary>
    public string? ActivationSignature { get; set; }

    /// <summary>
    /// Last heartbeat
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }

    /// <summary>
    /// Product information
    /// </summary>
    public ProductInfo? Product { get; set; }

    /// <summary>
    /// Consumer information
    /// </summary>
    public ConsumerInfo? Consumer { get; set; }
}

/// <summary>
/// Product key validation result
/// </summary>
public class ProductKeyValidationResult
{
    /// <summary>
    /// Validation success status
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// License ID if valid
    /// </summary>
    public Guid? LicenseId { get; set; }

    /// <summary>
    /// Current activation count
    /// </summary>
    public int CurrentActivations { get; set; }

    /// <summary>
    /// Maximum activations allowed
    /// </summary>
    public int MaxActivations { get; set; }

    /// <summary>
    /// Can be activated flag
    /// </summary>
    public bool CanActivate { get; set; }

    /// <summary>
    /// Validation error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// License validity period
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// License expiry date
    /// </summary>
    public DateTime? ValidTo { get; set; }

    public static ProductKeyValidationResult Valid(Guid licenseId, int currentActivations, int maxActivations, DateTime validFrom, DateTime validTo)
    {
        return new ProductKeyValidationResult
        {
            IsValid = true,
            LicenseId = licenseId,
            CurrentActivations = currentActivations,
            MaxActivations = maxActivations,
            CanActivate = currentActivations < maxActivations,
            ValidFrom = validFrom,
            ValidTo = validTo
        };
    }

    public static ProductKeyValidationResult Invalid(string errorMessage)
    {
        return new ProductKeyValidationResult
        {
            IsValid = false,
            CanActivate = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Simple product information
/// </summary>
public class ProductInfo
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductVersion { get; set; }
}

/// <summary>
/// Simple consumer information
/// </summary>
public class ConsumerInfo
{
    public Guid ConsumerId { get; set; }
    public string ConsumerName { get; set; } = string.Empty;
    public string? ConsumerEmail { get; set; }
}
