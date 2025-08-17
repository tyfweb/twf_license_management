using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.ProductKey;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using WebModels = TechWayFit.Licensing.Management.Web.Models.Api.ProductKey;
using CoreServices = TechWayFit.Licensing.Management.Core.Contracts.Services;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for Product Key Management
/// Provides REST API endpoints for Product Key activation workflow
/// Implements the 5-step Product Key license flow:
/// 1. Admin creates registration (ProductKey with PendingActivation status)
/// 2. Client calls activation API 
/// 3. System activates key and sets end date
/// 4. System generates unique signature for response
/// 5. Signature can be used to retrieve activation details
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductKeyApiController : BaseController
{
    private readonly ILogger<ProductKeyApiController> _logger;
    private readonly IProductActivationService _productActivationService;

    public ProductKeyApiController(
        ILogger<ProductKeyApiController> logger,
        IProductActivationService productActivationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _productActivationService = productActivationService ?? throw new ArgumentNullException(nameof(productActivationService));
    }

    /// <summary>
    /// Step 1: Admin creates a new Product Key registration
    /// Creates a Product Key with PendingActivation status
    /// </summary>
    /// <param name="request">Product key creation parameters</param>
    /// <returns>Created product key with PendingActivation status</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateProductKeyRegistration([FromBody] WebModels.CreateProductKeyRequest request)
    {
        try
        {
            _logger.LogInformation("Creating product key registration for ProductLicense {ProductLicenseId} by user {User}", 
                request.ProductLicenseId, CurrentUserName);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(JsonResponse.Error("Invalid request data", errors));
            }

            // Create product key registration request using the core service DTO
            var createRequest = new CoreServices.CreateProductKeyRequest
            {
                ProductId = Guid.NewGuid(), // This should come from the ProductLicense lookup
                ConsumerId = Guid.NewGuid(), // This should come from the ProductLicense lookup  
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddYears(1), // Default 1 year validity
                MaxActivations = 1,
                Metadata = request.Metadata?.ToDictionary(k => k.Key, v => (object)v.Value)
            };

            // Create product key with PendingActivation status
            var result = await _productActivationService.CreateProductKeyAsync(createRequest, CurrentUserName);

            if (result.Success)
            {
                var response = new ProductKeyRegistrationResponse
                {
                    Success = true,
                    ProductKey = result.ProductKey,
                    Status = ProductActivationStatus.PendingActivation,
                    CreatedAt = DateTime.UtcNow,
                    Message = "Product key registration created successfully. Key is pending activation."
                };

                _logger.LogInformation("Successfully created product key {ProductKey} for user {User}", 
                    result.ProductKey, CurrentUserName);

                return CreatedAtAction(nameof(GetProductKeyBySignature), 
                    new { signature = "pending" }, 
                    JsonResponse.OK(response, "Product key registration created successfully"));
            }
            else
            {
                var response = new ProductKeyRegistrationResponse
                {
                    Success = false,
                    Message = result.ErrorMessage,
                    Errors = result.ErrorMessage != null ? new List<string> { result.ErrorMessage } : new List<string>()
                };

                return BadRequest(JsonResponse.Error(result.ErrorMessage ?? "Failed to create product key", response.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product key registration for user {User}", CurrentUserName);
            return StatusCode(500, JsonResponse.Error("An unexpected error occurred while creating the product key registration"));
        }
    }

    /// <summary>
    /// Step 2 & 3: Client activates a Product Key
    /// System activates the key and sets activation end date
    /// </summary>
    /// <param name="request">Product key activation parameters</param>
    /// <returns>Activation result with unique signature</returns>
    [HttpPost("activate")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> ActivateProductKey([FromBody] ActivateProductKeyRequest request)
    {
        try
        {
            _logger.LogInformation("Activating product key {ProductKey} for client {ClientIdentifier}", 
                request.ProductKey, request.ClientIdentifier);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(JsonResponse.Error("Invalid request data", errors));
            }

            // Create activation request using the core service DTO
            var activationRequest = new CoreServices.ProductActivationRequest
            {
                ProductKey = request.ProductKey,
                MachineId = request.ClientIdentifier, // Map ClientIdentifier to MachineId
                MachineName = request.ClientIdentifier, // Use ClientIdentifier as machine name
                ActivationData = request.ClientInfo?.ToDictionary(k => k.Key, v => (object)v.Value)
            };

            // Step 3: Activate the key and set end date, generate signature
            var result = await _productActivationService.ActivateProductKeyAsync(activationRequest);

            if (result.Success)
            {
                var response = new ProductActivationResponse
                {
                    Success = true,
                    ActivationSignature = result.ActivationSignature, // Step 4: Unique signature
                    Status = ProductActivationStatus.Active,
                    ActivationDate = result.ActivationDetails?.ActivationDate,
                    ActivationEndDate = result.ActivationEndDate,
                    ProductKey = request.ProductKey,
                    Message = "Product key activated successfully"
                };

                _logger.LogInformation("Successfully activated product key {ProductKey} with signature {Signature}", 
                    request.ProductKey, result.ActivationSignature);

                return Ok(JsonResponse.OK(response, "Product key activated successfully"));
            }
            else
            {
                var response = new ProductActivationResponse
                {
                    Success = false,
                    ProductKey = request.ProductKey,
                    Message = result.ErrorMessage,
                    Errors = result.ErrorMessage != null ? new List<string> { result.ErrorMessage } : new List<string>()
                };

                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(JsonResponse.Error(result.ErrorMessage, response.Errors));
                }

                return BadRequest(JsonResponse.Error(result.ErrorMessage ?? "Failed to activate product key", response.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product key {ProductKey}", request.ProductKey);
            return StatusCode(500, JsonResponse.Error("An unexpected error occurred while activating the product key"));
        }
    }

    /// <summary>
    /// Step 5: Retrieve activation details using signature
    /// The signature from step 4 can be used to retrieve activation details
    /// </summary>
    /// <param name="signature">Unique activation signature</param>
    /// <returns>Complete activation details</returns>
    [HttpGet("activation/{signature}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetProductKeyBySignature(string signature)
    {
        try
        {
            _logger.LogInformation("Retrieving activation details for signature {Signature}", signature);

            if (string.IsNullOrWhiteSpace(signature))
            {
                return BadRequest(JsonResponse.Error("Activation signature is required"));
            }

            // Step 5: Use signature to retrieve details
            var result = await _productActivationService.GetActivationBySignatureAsync(signature);

            if (result != null)
            {
                var response = MapToActivationDetailsResponse(result);

                _logger.LogInformation("Successfully retrieved activation details for signature {Signature}", signature);

                return Ok(JsonResponse.OK(response, "Activation details retrieved successfully"));
            }
            else
            {
                return NotFound(JsonResponse.Error("Activation not found for the provided signature"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activation details for signature {Signature}", signature);
            return StatusCode(500, JsonResponse.Error("An unexpected error occurred while retrieving activation details"));
        }
    }

    /// <summary>
    /// Get all product keys with filtering and pagination
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <returns>Paginated list of product keys</returns>
    [HttpGet]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public Task<ActionResult<JsonResponse>> GetProductKeys([FromQuery] GetProductKeysRequest request)
    {
        try
        {
            _logger.LogInformation("Getting product keys with filters: Status={Status}, Client={Client}, Search={Search}", 
                request.Status, request.ClientIdentifier, request.SearchTerm);

            // Note: This would require implementing a GetProductActivationsAsync method in the service
            // For now, returning a placeholder structure

            var response = new GetProductKeysResponse
            {
                ProductKeys = new List<ProductActivationDetailsResponse>(),
                TotalCount = 0,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = 0
            };

            return Task.FromResult<ActionResult<JsonResponse>>(Ok(JsonResponse.OK(response, "Product keys retrieved successfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product keys");
            return Task.FromResult<ActionResult<JsonResponse>>(StatusCode(500, JsonResponse.Error("Failed to retrieve product keys")));
        }
    }

    /// <summary>
    /// Get product key statistics
    /// </summary>
    /// <returns>Summary statistics for product keys</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(500)]
    public Task<ActionResult<JsonResponse>> GetProductKeyStats()
    {
        try
        {
            _logger.LogInformation("Getting product key statistics");

            // Note: This would require implementing a GetProductActivationStatsAsync method in the service
            // For now, returning a placeholder structure

            var response = new ProductKeyStatsResponse
            {
                TotalKeys = 0,
                PendingActivationKeys = 0,
                ActiveKeys = 0,
                InactiveKeys = 0,
                ExpiredKeys = 0,
                RevokedKeys = 0,
                StatusBreakdown = new Dictionary<ProductActivationStatus, int>()
            };

            return Task.FromResult<ActionResult<JsonResponse>>(Ok(JsonResponse.OK(response, "Product key statistics retrieved successfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product key statistics");
            return Task.FromResult<ActionResult<JsonResponse>>(StatusCode(500, JsonResponse.Error("Failed to retrieve product key statistics")));
        }
    }

    /// <summary>
    /// Validate a product key without activating it
    /// </summary>
    /// <param name="productKey">Product key to validate</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/{productKey}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> ValidateProductKey(string productKey)
    {
        try
        {
            _logger.LogInformation("Validating product key {ProductKey}", productKey);

            if (string.IsNullOrWhiteSpace(productKey))
            {
                return BadRequest(JsonResponse.Error("Product key is required"));
            }

            // Use the actual validation method from the service
            var result = await _productActivationService.ValidateProductKeyAsync(productKey);
            
            var response = new
            {
                ProductKey = productKey,
                IsValid = result.IsValid,
                CanActivate = result.CanActivate,
                CurrentActivations = result.CurrentActivations,
                MaxActivations = result.MaxActivations,
                Status = result.IsValid ? ProductActivationStatus.PendingActivation : ProductActivationStatus.Revoked,
                Message = result.ErrorMessage ?? "Product key validation completed"
            };

            return Ok(JsonResponse.OK(response, "Product key validation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product key {ProductKey}", productKey);
            return StatusCode(500, JsonResponse.Error("An unexpected error occurred while validating the product key"));
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Map ProductActivationDetails to API response model
    /// </summary>
    private ProductActivationDetailsResponse MapToActivationDetailsResponse(CoreServices.ProductActivationDetails details)
    {
        return new ProductActivationDetailsResponse
        {
            Id = details.ActivationId,
            ProductKey = details.ProductKey,
            Status = details.Status,
            ClientIdentifier = details.MachineId, // Map MachineId to ClientIdentifier
            ActivationDate = details.ActivationDate,
            ActivationEndDate = details.ActivationEndDate,
            ActivationSignature = details.ActivationSignature,
            ActivationReason = "Automatic activation", // Default value since it's not in core model
            ClientInfo = new Dictionary<string, string>
            {
                { "MachineId", details.MachineId },
                { "MachineName", details.MachineName ?? "" }
            },
            Metadata = new Dictionary<string, string>(), // Empty for now
            CreatedAt = details.ActivationDate,
            UpdatedAt = details.LastHeartbeat,
            ProductLicenseId = details.LicenseId,
            LicenseCode = details.ProductKey, // Use ProductKey as LicenseCode for now
            LicenseStatus = LicenseStatus.Active, // Default to active
            LicenseValidFrom = details.ActivationDate,
            LicenseValidTo = details.ActivationEndDate ?? DateTime.UtcNow.AddYears(1)
        };
    }

    #endregion
}
