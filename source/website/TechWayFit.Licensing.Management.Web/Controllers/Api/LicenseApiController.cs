using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.License;
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

    public LicenseApiController(
        ILogger<LicenseApiController> logger,
        IProductLicenseService licenseService,
        IEnterpriseProductService productService,
        IConsumerAccountService consumerService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _consumerService = consumerService ?? throw new ArgumentNullException(nameof(consumerService));
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
        // try
        // {
        //     _logger.LogInformation("Downloading license file for {LicenseId}", id);

        //     var license = await _licenseService.GetLicenseByIdAsync(id);
        //     if (license == null)
        //     {
        //         return NotFound();
        //     }

        //     var licenseContent = await _licenseService.GenerateLicenseFileAsync(license);
        //     var fileName = $"license_{license.LicenseKey}.lic";

        //     return File(licenseContent, "application/octet-stream", fileName);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Error downloading license {LicenseId}", id);
        //     return StatusCode(500);
        // }
        throw new NotImplementedException("DownloadLicense method is not implemented yet.");
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
