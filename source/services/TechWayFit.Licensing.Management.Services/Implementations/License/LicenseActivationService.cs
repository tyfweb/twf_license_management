using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of license activation and usage tracking service
/// </summary>
public class LicenseActivationService : ILicenseActivationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductLicenseService _licenseService;
    private readonly ILogger<LicenseActivationService> _logger;

    public LicenseActivationService(
        IUnitOfWork unitOfWork,
        IProductLicenseService licenseService,
        ILogger<LicenseActivationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Activate a license with activation key
    /// </summary>
    public async Task<LicenseActivationResult> ActivateLicenseAsync(string activationKey, string deviceId, string deviceInfo)
    {
        try
        {
            _logger.LogInformation("Attempting license activation with key {ActivationKey} for device {DeviceId}", 
                activationKey?.Substring(0, Math.Min(8, activationKey?.Length ?? 0)), deviceId);

            // Validate input parameters
            if (string.IsNullOrEmpty(activationKey) || string.IsNullOrEmpty(deviceId))
            {
                return new LicenseActivationResult
                {
                    IsSuccessful = false,
                    Message = "Activation key and device ID are required",
                    ErrorCode = LicenseActivationError.InvalidLicenseKey
                };
            }

            // Find license by activation key
            var license = await _licenseService.GetLicenseByKeyAsync(activationKey);
            if (license == null)
            {
                _logger.LogWarning("License not found for activation key {ActivationKey}", activationKey);
                return new LicenseActivationResult
                {
                    IsSuccessful = false,
                    Message = "Invalid activation key",
                    ErrorCode = LicenseActivationError.InvalidLicenseKey
                };
            }

            // Check license status
            if (license.Status != Licensing.Core.Models.LicenseStatus.Active)
            {
                _logger.LogWarning("License {LicenseId} is not active (Status: {Status})", license.LicenseId, license.Status);
                return new LicenseActivationResult
                {
                    IsSuccessful = false,
                    Message = $"License is {license.Status.ToString().ToLower()}",
                    ErrorCode = GetErrorCodeForStatus(license.Status)
                };
            }

            // Check license expiration
            if (license.IsExpired)
            {
                _logger.LogWarning("License {LicenseId} has expired on {ExpiryDate}", license.LicenseId, license.ValidTo);
                return new LicenseActivationResult
                {
                    IsSuccessful = false,
                    Message = "License has expired",
                    ErrorCode = LicenseActivationError.LicenseExpired
                };
            }

            // Check if device can be activated
            var canActivate = await CanActivateOnDeviceAsync(license.LicenseId, deviceId);
            if (!canActivate)
            {
                _logger.LogWarning("Device {DeviceId} cannot be activated for license {LicenseId} - device limit exceeded", 
                    deviceId, license.LicenseId);
                return new LicenseActivationResult
                {
                    IsSuccessful = false,
                    Message = "Device activation limit exceeded",
                    ErrorCode = LicenseActivationError.DeviceLimitExceeded
                };
            }

            // Generate activation token
            var activationToken = GenerateActivationToken(license.LicenseId, deviceId);
            var activatedAt = DateTime.UtcNow;

            // TODO: Store activation record in database
            // This would typically create/update an activation record

            // Track activation usage
            await TrackUsageAsync(license.LicenseId, deviceId, LicenseUsageType.Activation, new Dictionary<string, object>
            {
                ["DeviceInfo"] = deviceInfo,
                ["ActivationToken"] = activationToken,
                ["ActivatedAt"] = activatedAt
            });

            _logger.LogInformation("License {LicenseId} successfully activated for device {DeviceId}", 
                license.LicenseId, deviceId);

            return new LicenseActivationResult
            {
                IsSuccessful = true,
                Message = "License activated successfully",
                LicenseId = license.LicenseId,
                ActivationToken = activationToken,
                ActivatedAt = activatedAt,
                ExpiresAt = license.ValidTo,
                Metadata = new Dictionary<string, object>
                {
                    ["ProductName"] = license.LicenseConsumer.Product.Name,
                    ["LicenseType"] = license.LicenseModel.ToString(),
                    ["Features"] = license.LicenseConsumer.Features.Where(f => f.IsEnabled).Select(f => f.Name).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating license with key {ActivationKey} for device {DeviceId}", 
                activationKey, deviceId);
            return new LicenseActivationResult
            {
                IsSuccessful = false,
                Message = "An error occurred during activation",
                ErrorCode = LicenseActivationError.ServerError
            };
        }
    }

    /// <summary>
    /// Validate an active license
    /// </summary>
    public async Task<LicenseValidationStatusResult> ValidateActiveLicenseAsync(Guid licenseId, string deviceId)
    {
        try
        {
            _logger.LogDebug("Validating license {LicenseId} for device {DeviceId}", licenseId, deviceId);

            var license = await _licenseService.GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                return new LicenseValidationStatusResult
                {
                    IsValid = false,
                    Message = "License not found",
                    Status = LicenseStatus.NotFound
                };
            }

            var warnings = new List<string>();
            var featureAccess = new Dictionary<string, bool>();

            // Check license status
            if (license.Status != LicenseStatus.Active)
            {
                return new LicenseValidationStatusResult
                {
                    IsValid = false,
                    Message = $"License is {license.Status.ToString().ToLower()}",
                    Status = license.Status,
                    LastValidated = DateTime.UtcNow
                };
            }

            // Check expiration
            var isExpired = license.IsExpired;
            var daysUntilExpiry = license.DaysUntilExpiry;

            if (isExpired)
            {
                return new LicenseValidationStatusResult
                {
                    IsValid = false,
                    Message = "License has expired",
                    Status = LicenseStatus.Expired,
                    ExpiresAt = license.ValidTo,
                    LastValidated = DateTime.UtcNow,
                    DaysUntilExpiry = daysUntilExpiry
                };
            }

            // Add expiration warnings
            if (daysUntilExpiry <= 30)
            {
                warnings.Add($"License expires in {daysUntilExpiry} days");
            }
            if (daysUntilExpiry <= 7)
            {
                warnings.Add("License expires soon - renewal recommended");
            }

            // Check device activation (if applicable)
            // TODO: Verify device is activated for this license

            // Build feature access map
            foreach (var feature in license.LicenseConsumer.Features)
            {
                featureAccess[feature.Name] = feature.IsEnabled;
            }

            // Track validation usage
            await TrackUsageAsync(licenseId, deviceId, LicenseUsageType.Validation);

            return new LicenseValidationStatusResult
            {
                IsValid = true,
                Message = "License is valid",
                Status = license.Status,
                LastValidated = DateTime.UtcNow,
                ExpiresAt = license.ValidTo,
                RequiresRenewal = daysUntilExpiry <= 30,
                DaysUntilExpiry = daysUntilExpiry,
                FeatureAccess = featureAccess,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license {LicenseId} for device {DeviceId}", licenseId, deviceId);
            return new LicenseValidationStatusResult
            {
                IsValid = false,
                Message = "Validation error occurred",
                Status = LicenseStatus.NotYetValid,
                LastValidated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Deactivate a license from a device
    /// </summary>
    public async Task<bool> DeactivateLicenseAsync(Guid licenseId, string deviceId, string reason = "User requested")
    {
        try
        {
            _logger.LogInformation("Deactivating license {LicenseId} from device {DeviceId}. Reason: {Reason}", 
                licenseId, deviceId, reason);

            // TODO: Remove activation record from database
            // This would typically update/delete the activation record

            // Track deactivation usage
            await TrackUsageAsync(licenseId, deviceId, LicenseUsageType.Deactivation, new Dictionary<string, object>
            {
                ["Reason"] = reason,
                ["DeactivatedAt"] = DateTime.UtcNow
            });

            _logger.LogInformation("License {LicenseId} successfully deactivated from device {DeviceId}", 
                licenseId, deviceId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating license {LicenseId} from device {DeviceId}", licenseId, deviceId);
            return false;
        }
    }

    /// <summary>
    /// Track license usage activity
    /// </summary>
    public async Task<bool> TrackUsageAsync(Guid licenseId, string deviceId, LicenseUsageType usageType, Dictionary<string, object>? metadata = null)
    {
        try
        {
            // TODO: Store usage record in database
            // This would typically create a usage tracking record
            
            _logger.LogDebug("Tracking usage for license {LicenseId}, device {DeviceId}, type {UsageType}", 
                licenseId, deviceId, usageType);

            await Task.CompletedTask; // Placeholder for actual implementation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking usage for license {LicenseId}", licenseId);
            return false;
        }
    }

    /// <summary>
    /// Get license usage statistics
    /// </summary>
    public async Task<LicenseUsageStats> GetUsageStatsAsync(Guid licenseId)
    {
        try
        {
            // TODO: Query usage statistics from database
            await Task.CompletedTask; // Placeholder

            return new LicenseUsageStats
            {
                LicenseId = licenseId,
                TotalActivations = 0,
                ActiveDevices = 0,
                MaxAllowedDevices = 1, // TODO: Get from license configuration
                FirstActivation = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                TotalUsageHours = 0,
                UsageByType = new Dictionary<string, int>(),
                RecentActivity = new List<LicenseUsageRecord>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats for license {LicenseId}", licenseId);
            throw;
        }
    }

    /// <summary>
    /// Get all active devices for a license
    /// </summary>
    public async Task<List<LicenseDevice>> GetActiveDevicesAsync(Guid licenseId)
    {
        try
        {
            // TODO: Query active devices from database
            await Task.CompletedTask; // Placeholder

            return new List<LicenseDevice>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active devices for license {LicenseId}", licenseId);
            throw;
        }
    }

    /// <summary>
    /// Check if license can be activated on additional devices
    /// </summary>
    public async Task<bool> CanActivateOnDeviceAsync(Guid licenseId, string deviceId)
    {
        try
        {
            var license = await _licenseService.GetLicenseByIdAsync(licenseId);
            if (license == null)
                return false;

            // TODO: Check actual device count from database
            var activeDevices = await GetActiveDevicesAsync(licenseId);
            var maxAllowedDevices = license.MaxAllowedUsers ?? 1; // Default to 1 device

            // Check if device is already activated
            var deviceAlreadyActivated = activeDevices.Any(d => d.DeviceId == deviceId && d.IsActive);
            if (deviceAlreadyActivated)
                return true; // Device is already activated

            // Check if we can add a new device
            return activeDevices.Count(d => d.IsActive) < maxAllowedDevices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking device activation eligibility for license {LicenseId}", licenseId);
            return false;
        }
    }

    /// <summary>
    /// Generate offline activation request
    /// </summary>
    public async Task<OfflineActivationRequest> GenerateOfflineActivationRequestAsync(string licenseKey, string deviceId)
    {
        try
        {
            var deviceFingerprint = GenerateDeviceFingerprint(deviceId);
            var challenge = GenerateActivationChallenge(licenseKey, deviceId);

            await Task.CompletedTask; // Placeholder for async operations

            return new OfflineActivationRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                LicenseKey = licenseKey,
                DeviceId = deviceId,
                DeviceFingerprint = deviceFingerprint,
                RequestedAt = DateTime.UtcNow,
                ActivationChallenge = challenge,
                DeviceInfo = new Dictionary<string, object>
                {
                    ["OS"] = Environment.OSVersion.ToString(),
                    ["MachineName"] = Environment.MachineName,
                    ["UserName"] = Environment.UserName,
                    ["ProcessorCount"] = Environment.ProcessorCount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating offline activation request for license {LicenseKey}", licenseKey);
            throw;
        }
    }

    /// <summary>
    /// Process offline activation response
    /// </summary>
    public async Task<LicenseActivationResult> ProcessOfflineActivationResponseAsync(string activationResponse)
    {
        try
        {
            // TODO: Implement offline activation response processing
            // This would typically decrypt and validate the activation response
            await Task.CompletedTask; // Placeholder

            return new LicenseActivationResult
            {
                IsSuccessful = false,
                Message = "Offline activation not yet implemented",
                ErrorCode = LicenseActivationError.ServerError
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline activation response");
            return new LicenseActivationResult
            {
                IsSuccessful = false,
                Message = "Error processing activation response",
                ErrorCode = LicenseActivationError.ServerError
            };
        }
    }

    /// <summary>
    /// Refresh license status and sync with server
    /// </summary>
    public async Task<LicenseRefreshResult> RefreshLicenseAsync(Guid licenseId, string deviceId)
    {
        try
        {
            _logger.LogInformation("Refreshing license {LicenseId} for device {DeviceId}", licenseId, deviceId);

            var license = await _licenseService.GetLicenseByIdAsync(licenseId);
            if (license == null)
            {
                return new LicenseRefreshResult
                {
                    IsSuccessful = false,
                    Message = "License not found"
                };
            }

            // Check for license changes and updates
            var changes = new List<string>();
            
            // TODO: Compare with cached license information to detect changes
            // This would typically check for status changes, expiration updates, etc.

            return new LicenseRefreshResult
            {
                IsSuccessful = true,
                Message = "License refreshed successfully",
                LicenseUpdated = changes.Any(),
                LastRefresh = DateTime.UtcNow,
                Status = license.Status,
                Changes = changes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing license {LicenseId}", licenseId);
            return new LicenseRefreshResult
            {
                IsSuccessful = false,
                Message = "Error refreshing license"
            };
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Generate activation token for device
    /// </summary>
    private static string GenerateActivationToken(Guid licenseId, string deviceId)
    {
        var data = $"{licenseId}:{deviceId}:{DateTime.UtcNow:yyyyMMddHHmm}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Generate device fingerprint
    /// </summary>
    private static string GenerateDeviceFingerprint(string deviceId)
    {
        var data = $"{deviceId}:{Environment.MachineName}:{Environment.OSVersion}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Generate activation challenge for offline activation
    /// </summary>
    private static string GenerateActivationChallenge(string licenseKey, string deviceId)
    {
        var data = $"{licenseKey}:{deviceId}:{DateTime.UtcNow.Ticks}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Get error code for license status
    /// </summary>
    private static LicenseActivationError GetErrorCodeForStatus(LicenseStatus status)
    {
        return status switch
        {
            LicenseStatus.Expired => LicenseActivationError.LicenseExpired,
            LicenseStatus.Revoked => LicenseActivationError.LicenseRevoked,
            LicenseStatus.Suspended => LicenseActivationError.LicenseInactive,
            LicenseStatus.Invalid => LicenseActivationError.LicenseInactive,
            _ => LicenseActivationError.InvalidLicenseKey
        };
    }

    #endregion
}
