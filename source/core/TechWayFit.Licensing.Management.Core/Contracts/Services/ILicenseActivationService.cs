using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service for license activation and usage tracking
/// </summary>
public interface ILicenseActivationService
{
    /// <summary>
    /// Activate a license with activation key
    /// </summary>
    Task<LicenseActivationResult> ActivateLicenseAsync(string activationKey, string deviceId, string deviceInfo);

    /// <summary>
    /// Validate an active license
    /// </summary>
    Task<LicenseValidationStatusResult> ValidateActiveLicenseAsync(Guid licenseId, string deviceId);

    /// <summary>
    /// Deactivate a license from a device
    /// </summary>
    Task<bool> DeactivateLicenseAsync(Guid licenseId, string deviceId, string reason = "User requested");

    /// <summary>
    /// Track license usage activity
    /// </summary>
    Task<bool> TrackUsageAsync(Guid licenseId, string deviceId, LicenseUsageType usageType, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Get license usage statistics
    /// </summary>
    Task<LicenseUsageStats> GetUsageStatsAsync(Guid licenseId);

    /// <summary>
    /// Get all active devices for a license
    /// </summary>
    Task<List<LicenseDevice>> GetActiveDevicesAsync(Guid licenseId);

    /// <summary>
    /// Check if license can be activated on additional devices
    /// </summary>
    Task<bool> CanActivateOnDeviceAsync(Guid licenseId, string deviceId);

    /// <summary>
    /// Generate offline activation request
    /// </summary>
    Task<OfflineActivationRequest> GenerateOfflineActivationRequestAsync(string licenseKey, string deviceId);

    /// <summary>
    /// Process offline activation response
    /// </summary>
    Task<LicenseActivationResult> ProcessOfflineActivationResponseAsync(string activationResponse);

    /// <summary>
    /// Refresh license status and sync with server
    /// </summary>
    Task<LicenseRefreshResult> RefreshLicenseAsync(Guid licenseId, string deviceId);
}

/// <summary>
/// Result of license activation attempt
/// </summary>
public class LicenseActivationResult
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? LicenseId { get; set; }
    public string? ActivationToken { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public LicenseActivationError? ErrorCode { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of license validation
/// </summary>
public class LicenseValidationStatusResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public LicenseStatus Status { get; set; }
    public DateTime? LastValidated { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresRenewal { get; set; }
    public int DaysUntilExpiry { get; set; }
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// License usage statistics
/// </summary>
public class LicenseUsageStats
{
    public Guid LicenseId { get; set; }
    public int TotalActivations { get; set; }
    public int ActiveDevices { get; set; }
    public int MaxAllowedDevices { get; set; }
    public DateTime FirstActivation { get; set; }
    public DateTime LastActivity { get; set; }
    public long TotalUsageHours { get; set; }
    public Dictionary<string, int> UsageByType { get; set; } = new();
    public List<LicenseUsageRecord> RecentActivity { get; set; } = new();
}

/// <summary>
/// Device information for license activation
/// </summary>
public class LicenseDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public DateTime ActivatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public bool IsActive { get; set; }
    public string? IpAddress { get; set; }
    public Dictionary<string, object> DeviceInfo { get; set; } = new();
}

/// <summary>
/// Usage record for tracking license activity
/// </summary>
public class LicenseUsageRecord
{
    public Guid Id { get; set; }
    public Guid LicenseId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public LicenseUsageType UsageType { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Offline activation request
/// </summary>
public class OfflineActivationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string ActivationChallenge { get; set; } = string.Empty;
    public Dictionary<string, object> DeviceInfo { get; set; } = new();
}

/// <summary>
/// License refresh result
/// </summary>
public class LicenseRefreshResult
{
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool LicenseUpdated { get; set; }
    public DateTime? LastRefresh { get; set; }
    public LicenseStatus Status { get; set; }
    public List<string> Changes { get; set; } = new();
}

/// <summary>
/// License activation error codes
/// </summary>
public enum LicenseActivationError
{
    None = 0,
    InvalidLicenseKey = 1,
    LicenseExpired = 2,
    LicenseRevoked = 3,
    LicenseInactive = 4,
    DeviceLimitExceeded = 5,
    DeviceAlreadyActivated = 6,
    InvalidDeviceId = 7,
    NetworkError = 8,
    ServerError = 9,
    InvalidActivationToken = 10,
    ActivationLocked = 11,
    UnauthorizedDevice = 12
}

/// <summary>
/// License usage types for tracking
/// </summary>
public enum LicenseUsageType
{
    Activation = 1,
    Deactivation = 2,
    Validation = 3,
    FeatureAccess = 4,
    ApplicationStart = 5,
    ApplicationStop = 6,
    HeartBeat = 7,
    OfflineUsage = 8,
    LicenseRefresh = 9,
    Error = 10
}
