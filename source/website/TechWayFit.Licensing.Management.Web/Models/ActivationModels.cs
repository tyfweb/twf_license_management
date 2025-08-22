using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.Models;

/// <summary>
/// Request model for license activation
/// </summary>
public class ActivationRequest
{
    [Required(ErrorMessage = "Activation key is required")]
    [StringLength(100, ErrorMessage = "Activation key cannot exceed 100 characters")]
    public string ActivationKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Device info cannot exceed 500 characters")]
    public string? DeviceInfo { get; set; }
}

/// <summary>
/// Request model for license validation
/// </summary>
public class ValidationRequest
{
    [Required(ErrorMessage = "License ID is required")]
    public Guid LicenseId { get; set; }

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for license deactivation
/// </summary>
public class DeactivationRequest
{
    [Required(ErrorMessage = "License ID is required")]
    public Guid LicenseId { get; set; }

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for offline activation request generation
/// </summary>
public class OfflineActivationRequestModel
{
    [Required(ErrorMessage = "License key is required")]
    [StringLength(100, ErrorMessage = "License key cannot exceed 100 characters")]
    public string LicenseKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for offline activation response processing
/// </summary>
public class OfflineActivationResponseModel
{
    [Required(ErrorMessage = "Activation response is required")]
    public string ActivationResponse { get; set; } = string.Empty;
}

/// <summary>
/// Request model for license refresh
/// </summary>
public class RefreshRequest
{
    [Required(ErrorMessage = "License ID is required")]
    public Guid LicenseId { get; set; }

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for license usage heartbeat
/// </summary>
public class HeartbeatRequest
{
    [Required(ErrorMessage = "License ID is required")]
    public Guid LicenseId { get; set; }

    [Required(ErrorMessage = "Device ID is required")]
    [StringLength(200, ErrorMessage = "Device ID cannot exceed 200 characters")]
    public string DeviceId { get; set; } = string.Empty;

    public Dictionary<string, object>? Metadata { get; set; }
}
