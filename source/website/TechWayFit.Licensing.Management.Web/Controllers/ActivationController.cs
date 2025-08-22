using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.Models;

namespace TechWayFit.Licensing.Management.Web.Controllers;

/// <summary>
/// API Controller for license activation and validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ActivationController : ControllerBase
{
    private readonly ILicenseActivationService _activationService;
    private readonly ILogger<ActivationController> _logger;

    public ActivationController(
        ILicenseActivationService activationService,
        ILogger<ActivationController> logger)
    {
        _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Activate a license with activation key
    /// </summary>
    [HttpPost("activate")]
    public async Task<IActionResult> ActivateLicense([FromBody] ActivationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.ActivateLicenseAsync(
                request.ActivationKey, 
                request.DeviceId, 
                request.DeviceInfo ?? "Unknown Device");

            if (result.IsSuccessful)
            {
                _logger.LogInformation("License activated successfully for device {DeviceId}", request.DeviceId);
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    licenseId = result.LicenseId,
                    activationToken = result.ActivationToken,
                    activatedAt = result.ActivatedAt,
                    expiresAt = result.ExpiresAt,
                    metadata = result.Metadata
                });
            }

            _logger.LogWarning("License activation failed for device {DeviceId}: {Error}", 
                request.DeviceId, result.Message);

            return BadRequest(new
            {
                success = false,
                message = result.Message,
                errorCode = result.ErrorCode?.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during license activation for device {DeviceId}", request?.DeviceId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during activation"
            });
        }
    }

    /// <summary>
    /// Validate an active license
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateLicense([FromBody] ValidationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.ValidateActiveLicenseAsync(request.LicenseId, request.DeviceId);

            return Ok(new
            {
                isValid = result.IsValid,
                message = result.Message,
                status = result.Status.ToString(),
                lastValidated = result.LastValidated,
                expiresAt = result.ExpiresAt,
                requiresRenewal = result.RequiresRenewal,
                daysUntilExpiry = result.DaysUntilExpiry,
                featureAccess = result.FeatureAccess,
                warnings = result.Warnings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during license validation for license {LicenseId}", request?.LicenseId);
            return StatusCode(500, new
            {
                isValid = false,
                message = "An error occurred during validation"
            });
        }
    }

    /// <summary>
    /// Deactivate a license from a device
    /// </summary>
    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateLicense([FromBody] DeactivationRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.DeactivateLicenseAsync(
                request.LicenseId, 
                request.DeviceId, 
                request.Reason ?? "User requested");

            if (result)
            {
                _logger.LogInformation("License {LicenseId} deactivated from device {DeviceId}", 
                    request.LicenseId, request.DeviceId);
                return Ok(new
                {
                    success = true,
                    message = "License deactivated successfully"
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Failed to deactivate license"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during license deactivation for license {LicenseId}", request?.LicenseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during deactivation"
            });
        }
    }

    /// <summary>
    /// Get license usage statistics
    /// </summary>
    [HttpGet("{licenseId}/usage-stats")]
    public async Task<IActionResult> GetUsageStats(Guid licenseId)
    {
        try
        {
            var stats = await _activationService.GetUsageStatsAsync(licenseId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats for license {LicenseId}", licenseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving usage statistics"
            });
        }
    }

    /// <summary>
    /// Get active devices for a license
    /// </summary>
    [HttpGet("{licenseId}/devices")]
    public async Task<IActionResult> GetActiveDevices(Guid licenseId)
    {
        try
        {
            var devices = await _activationService.GetActiveDevicesAsync(licenseId);
            return Ok(new
            {
                licenseId = licenseId,
                activeDevices = devices.Count,
                devices = devices
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active devices for license {LicenseId}", licenseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while retrieving device information"
            });
        }
    }

    /// <summary>
    /// Generate offline activation request
    /// </summary>
    [HttpPost("offline/request")]
    public async Task<IActionResult> GenerateOfflineActivationRequest([FromBody] OfflineActivationRequestModel request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var offlineRequest = await _activationService.GenerateOfflineActivationRequestAsync(
                request.LicenseKey, 
                request.DeviceId);

            return Ok(new
            {
                success = true,
                requestId = offlineRequest.RequestId,
                activationChallenge = offlineRequest.ActivationChallenge,
                deviceFingerprint = offlineRequest.DeviceFingerprint,
                requestedAt = offlineRequest.RequestedAt,
                instructions = "Please send this activation challenge to support for manual activation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating offline activation request");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while generating offline activation request"
            });
        }
    }

    /// <summary>
    /// Process offline activation response
    /// </summary>
    [HttpPost("offline/activate")]
    public async Task<IActionResult> ProcessOfflineActivation([FromBody] OfflineActivationResponseModel request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.ProcessOfflineActivationResponseAsync(request.ActivationResponse);

            if (result.IsSuccessful)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    licenseId = result.LicenseId,
                    activationToken = result.ActivationToken,
                    activatedAt = result.ActivatedAt,
                    expiresAt = result.ExpiresAt
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.Message,
                errorCode = result.ErrorCode?.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing offline activation");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while processing offline activation"
            });
        }
    }

    /// <summary>
    /// Refresh license status
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshLicense([FromBody] RefreshRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.RefreshLicenseAsync(request.LicenseId, request.DeviceId);

            return Ok(new
            {
                success = result.IsSuccessful,
                message = result.Message,
                licenseUpdated = result.LicenseUpdated,
                lastRefresh = result.LastRefresh,
                status = result.Status.ToString(),
                changes = result.Changes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing license {LicenseId}", request?.LicenseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while refreshing license"
            });
        }
    }

    /// <summary>
    /// Track license usage (heartbeat)
    /// </summary>
    [HttpPost("heartbeat")]
    public async Task<IActionResult> TrackHeartbeat([FromBody] HeartbeatRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _activationService.TrackUsageAsync(
                request.LicenseId, 
                request.DeviceId, 
                LicenseUsageType.HeartBeat,
                request.Metadata);

            return Ok(new
            {
                success = result,
                message = result ? "Heartbeat recorded" : "Failed to record heartbeat",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking heartbeat for license {LicenseId}", request?.LicenseId);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while tracking usage"
            });
        }
    }
}
