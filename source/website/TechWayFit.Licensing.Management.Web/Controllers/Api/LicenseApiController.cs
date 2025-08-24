using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.License;
using TechWayFit.Licensing.Management.Web.Controllers;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for License Management
/// Provides REST API endpoints for license operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LicenseApiController : BaseController
{
    private readonly ILogger<LicenseApiController> _logger;
    private readonly IProductLicenseService _licenseService;
    private readonly IEnterpriseProductService _productService;
    private readonly IConsumerAccountService _consumerService;
    private readonly ILicenseFileService _licenseFileService;

    public LicenseApiController(
        ILogger<LicenseApiController> logger,
        IProductLicenseService licenseService,
        IEnterpriseProductService productService,
        IConsumerAccountService consumerService,
        ILicenseFileService licenseFileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _consumerService = consumerService ?? throw new ArgumentNullException(nameof(consumerService));
        _licenseFileService = licenseFileService ?? throw new ArgumentNullException(nameof(licenseFileService));
    }

    /// <summary>
    /// Get all licenses with filtering and pagination
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <returns>Paginated list of licenses</returns>
    [HttpGet]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetLicenses([FromQuery] GetLicensesRequest request)
    {
        try
        {
            _logger.LogInformation("Getting licenses with filters: SearchTerm={SearchTerm}, Status={Status}", 
                request.SearchTerm, request.Status);

            var licenses = await _licenseService.GetLicensesAsync(
                status: request.Status,
                searchTerm: request.SearchTerm,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var response = new GetLicensesResponse
            {
                Licenses = licenses.Select(MapToLicenseResponse).ToList(),
                TotalCount = licenses.Count(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)licenses.Count() / request.PageSize)
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting licenses");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve licenses"));
        }
    }

    /// <summary>
    /// Get license by ID
    /// </summary>
    /// <param name="id">License ID</param>
    /// <returns>License details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetLicense(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting license with ID: {LicenseId}", id);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            var response = MapToLicenseResponse(license);
            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve license"));
        }
    }
  

    /// <summary>
    /// Validate a license key
    /// </summary>
    /// <param name="request">License validation parameters</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> ValidateLicense([FromBody] ValidateLicenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Validating license key: {LicenseKey}", request.LicenseKey.Substring(0, Math.Min(8, request.LicenseKey.Length)) + "...");

            var license = await _licenseService.GetLicenseByKeyAsync(request.LicenseKey);
            
            var response = new ValidateLicenseResponse();
            
            if (license == null)
            {
                response.IsValid = false;
                response.Message = "License key not found";
                response.Errors.Add("Invalid license key");
            }
            else if (license.Status != LicenseStatus.Active)
            {
                response.IsValid = false;
                response.Message = $"License is {license.Status}";
                response.Errors.Add($"License status is {license.Status}");
            }
            else if (license.ValidTo < DateTime.UtcNow)
            {
                response.IsValid = false;
                response.Message = "License has expired";
                response.Errors.Add($"License expired on {license.ValidTo:yyyy-MM-dd}");
            }
            else
            {
                response.IsValid = true;
                response.Message = "License is valid";
                response.License = MapToLicenseResponse(license);
            }

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license");
            return StatusCode(500, JsonResponse.Error("Failed to validate license"));
        }
    }

    /// <summary>
    /// Download license file
    /// </summary>
    /// <param name="id">License ID</param>
    /// <returns>License file</returns>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DownloadLicense(Guid id)
    {
        try
        {
            _logger.LogInformation("Downloading license file for {LicenseId}", id);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                _logger.LogWarning("License {LicenseId} not found", id);
                return NotFound(JsonResponse.Error("License not found"));
            }

            // Check if license is in a downloadable state
            if (license.Status != LicenseStatus.Active)
            {
                _logger.LogWarning("Attempted to download license {LicenseId} with status {Status}", id, license.Status);
                return BadRequest(JsonResponse.Error("Only active licenses can be downloaded"));
            }

            // Generate license file content
            var licenseContent = await _licenseFileService.GenerateLicenseFileAsync(license);
            var fileName = $"license_{license.LicenseKey}_{DateTime.UtcNow:yyyyMMdd}.lic";

            // Track the download
            await _licenseFileService.TrackDownloadAsync(license.LicenseId, User.Identity?.Name ?? "API User", "lic");

            // Log the download action
            _logger.LogInformation("License {LicenseId} downloaded via API by user {User}", id, User.Identity?.Name);

            // Convert to bytes and return as file
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(licenseContent);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to download license"));
        }
    }

    /// <summary>
    /// Suspend a license
    /// </summary>
    /// <param name="id">License ID</param>
    /// <param name="request">Suspend request parameters</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> SuspendLicense(Guid id, [FromBody] SuspendLicenseRequest request)
    {
        try
        {
            _logger.LogInformation("Suspending license {LicenseId} with reason: {Reason}", id, request.Reason);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            var result = await _licenseService.SuspendLicenseAsync(id, User.Identity?.Name ?? "API User", request.Reason);
            
            if (result)
            {
                _logger.LogInformation("License {LicenseId} suspended successfully", id);
                return Ok(JsonResponse.OK("License suspended successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to suspend license {LicenseId}", id);
                return BadRequest(JsonResponse.Error("Failed to suspend license"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to suspend license"));
        }
    }

    /// <summary>
    /// Reactivate a license
    /// </summary>
    /// <param name="id">License ID</param>
    /// <param name="request">Reactivate request parameters</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> ReactivateLicense(Guid id, [FromBody] ReactivateLicenseRequest request)
    {
        try
        {
            _logger.LogInformation("Reactivating license {LicenseId} with reason: {Reason}", id, request.Reason);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            // Create ActivationInfo for reactivation
            var activationInfo = new ActivationInfo
            {
                ActivatedBy = User.Identity?.Name ?? "API User",
                ActivationDate = DateTime.UtcNow,
                ActivationMetadata = new Dictionary<string, object>
                {
                    { "reason", request.Reason ?? "License reactivated via API" },
                    { "apiEndpoint", "reactivate" }
                }
            };

            var result = await _licenseService.ActivateLicenseAsync(id, activationInfo);
            
            if (result)
            {
                _logger.LogInformation("License {LicenseId} reactivated successfully", id);
                return Ok(JsonResponse.OK("License reactivated successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to reactivate license {LicenseId}", id);
                return BadRequest(JsonResponse.Error("Failed to reactivate license"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to reactivate license"));
        }
    }

    /// <summary>
    /// Renew a license
    /// </summary>
    /// <param name="id">License ID</param>
    /// <param name="request">Renew request parameters</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> RenewLicense(Guid id, [FromBody] RenewLicenseRequest request)
    {
        try
        {
            _logger.LogInformation("Renewing license {LicenseId} for {Duration} days", id, request.RenewalDurationDays);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            // Calculate new expiry date
            var currentExpiry = license.ValidTo;
            var newExpiryDate = currentExpiry > DateTime.UtcNow 
                ? currentExpiry.AddDays(request.RenewalDurationDays)
                : DateTime.UtcNow.AddDays(request.RenewalDurationDays);

            var result = await _licenseService.RenewLicenseAsync(id, newExpiryDate, User.Identity?.Name ?? "API User");
            
            if (result)
            {
                _logger.LogInformation("License {LicenseId} renewed successfully", id);
                var response = new RenewLicenseResponse
                {
                    Message = "License renewed successfully",
                    NewExpirationDate = newExpiryDate,
                    RenewalDurationDays = request.RenewalDurationDays
                };
                return Ok(JsonResponse.OK(response));
            }
            else
            {
                _logger.LogWarning("Failed to renew license {LicenseId}", id);
                return BadRequest(JsonResponse.Error("Failed to renew license"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to renew license"));
        }
    }

    /// <summary>
    /// Revoke a license
    /// </summary>
    /// <param name="id">License ID</param>
    /// <param name="request">Revoke request parameters</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id:guid}/revoke")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> RevokeLicense(Guid id, [FromBody] RevokeLicenseRequest request)
    {
        try
        {
            _logger.LogInformation("Revoking license {LicenseId} with reason: {Reason}", id, request.Reason);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            var result = await _licenseService.RevokeLicenseAsync(id, User.Identity?.Name ?? "API User", request.Reason);
            
            if (result)
            {
                _logger.LogInformation("License {LicenseId} revoked successfully", id);
                return Ok(JsonResponse.OK("License revoked successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to revoke license {LicenseId}", id);
                return BadRequest(JsonResponse.Error("Failed to revoke license"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking license {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to revoke license"));
        }
    }

    /// <summary>
    /// Regenerate license key
    /// </summary>
    /// <param name="id">License ID</param>
    /// <param name="request">Regenerate request parameters</param>
    /// <returns>Operation result with new license key</returns>
    [HttpPost("{id:guid}/regenerate-key")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> RegenerateLicenseKey(Guid id, [FromBody] RegenerateLicenseKeyRequest request)
    {
        try
        {
            _logger.LogInformation("Regenerating license key for {LicenseId} with reason: {Reason}", id, request.Reason);

            var license = await _licenseService.GetLicenseByIdAsync(id);
            if (license == null)
            {
                return NotFound(JsonResponse.Error($"License with ID {id} not found"));
            }

            var result = await _licenseService.RegenerateLicenseKeyAsync(id, User.Identity?.Name ?? "API User", request.Reason);
            
            if (result != null)
            {
                _logger.LogInformation("License key regenerated successfully for {LicenseId}", id);
                var response = new RegenerateLicenseKeyResponse
                {
                    Message = "License key regenerated successfully",
                    NewLicenseKey = result.LicenseKey,
                    RegenerationReason = request.Reason
                };
                return Ok(JsonResponse.OK(response));
            }
            else
            {
                _logger.LogWarning("Failed to regenerate license key for {LicenseId}", id);
                return BadRequest(JsonResponse.Error("Failed to regenerate license key"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating license key for {LicenseId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to regenerate license key"));
        }
    }
 
    private LicenseResponse MapToLicenseResponse(ProductLicense license)
    {
        return new LicenseResponse
        {
            Id = license.Id,
            LicenseKey = license.LicenseKey,
            Product = new LicenseResponse.ProductInfo
            {
                Id = license.ProductId,
                Name = license.LicenseConsumer.Product?.Name ?? "",
                Version = license.LicenseConsumer.Product?.Version ?? ""
            },
            Consumer = new LicenseResponse.ConsumerInfo
            {
                Id = license.ConsumerId,
                Name = license.LicenseConsumer.Consumer?.CompanyName ?? "",
                Email = license.LicenseConsumer.Consumer?.PrimaryContact?.Email ?? "",
                Company = license.LicenseConsumer.Consumer?.CompanyName
            },
            CreatedDate = license.Audit.CreatedOn,
            ExpirationDate = license.ValidTo,
            Status = license.Status,
            MaxUsers = 0,
            TierName = license.LicenseConsumer.ProductTier.Name,
           // Features = license.Features?.Select(f => f.Name).ToList() ?? new List<string>()
        };
    }
}
