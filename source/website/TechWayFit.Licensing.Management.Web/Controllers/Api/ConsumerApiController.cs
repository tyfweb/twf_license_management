using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Web.Models.Api;
using TechWayFit.Licensing.Management.Web.Models.Api.Consumer;
using TechWayFit.Licensing.Management.Web.Controllers;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Web.Controllers.Api;

/// <summary>
/// API Controller for Consumer Management
/// Provides REST API endpoints for consumer operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ConsumerApiController : BaseController
{
    private readonly ILogger<ConsumerApiController> _logger;
    private readonly IConsumerAccountService _consumerService;
    private readonly IProductLicenseService _licenseService;

    public ConsumerApiController(
        ILogger<ConsumerApiController> logger,
        IConsumerAccountService consumerService,
        IProductLicenseService licenseService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _consumerService = consumerService ?? throw new ArgumentNullException(nameof(consumerService));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
    }

    /// <summary>
    /// Get all consumers with filtering and pagination
    /// </summary>
    /// <param name="request">Filter parameters</param>
    /// <returns>Paginated list of consumers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetConsumers([FromQuery] GetConsumersRequest request)
    {
        try
        {
            _logger.LogInformation("Getting consumers with filters: SearchTerm={SearchTerm}, Status={Status}", 
                request.SearchTerm, request.Status);

            var consumers = await _consumerService.GetConsumerAccountsAsync(
                status: request.Status,
                isActive: request.IsActive,
                searchTerm: request.SearchTerm,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            var response = new GetConsumersResponse
            {
                Consumers = consumers.Select(MapToConsumerResponse).ToList(),
                TotalCount = consumers.Count(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)consumers.Count() / request.PageSize)
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumers");
            return StatusCode(500, JsonResponse.Error("Failed to retrieve consumers"));
        }
    }

    /// <summary>
    /// Get consumer by ID
    /// </summary>
    /// <param name="id">Consumer ID</param>
    /// <returns>Consumer details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetConsumer(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting consumer with ID: {ConsumerId}", id);

            var consumer = await _consumerService.GetConsumerAccountByIdAsync(id);
            if (consumer == null)
            {
                return NotFound(JsonResponse.Error($"Consumer with ID {id} not found"));
            }

            var response = MapToConsumerResponse(consumer);
            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer {ConsumerId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve consumer"));
        }
    }

    /// <summary>
    /// Create a new consumer
    /// </summary>
    /// <param name="request">Consumer creation parameters</param>
    /// <returns>Created consumer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateConsumer([FromBody] CreateConsumerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            _logger.LogInformation("Creating consumer: {ConsumerName}", request.Name);

            var consumer = new ConsumerAccount
            {
                CompanyName = request.Name,
                PrimaryContact = new ContactPerson
                {
                    Email = request.Email,
                    Name = request.Name,
                    Phone = request.Phone
                },
                Address = new Address
                {
                    Street = request.Address ?? "",
                    City = request.City ?? "",
                    Country = request.Country ?? "",
                    PostalCode = request.PostalCode ?? ""
                },
                Status = request.Status, 
                Audit = new AuditInfo
                {
                    CreatedBy = GetCurrentUserId(),
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                }
            };

            var createdConsumer = await _consumerService.CreateConsumerAccountAsync(consumer, CurrentUserName);
            var response = MapToConsumerResponse(createdConsumer);

            return CreatedAtAction(nameof(GetConsumer), new { id = createdConsumer.ConsumerId }, 
                JsonResponse.OK(response, "Consumer created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumer");
            return StatusCode(500, JsonResponse.Error("Failed to create consumer"));
        }
    }

    /// <summary>
    /// Update an existing consumer
    /// </summary>
    /// <param name="id">Consumer ID</param>
    /// <param name="request">Consumer update parameters</param>
    /// <returns>Updated consumer</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> UpdateConsumer(Guid id, [FromBody] UpdateConsumerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            _logger.LogInformation("Updating consumer {ConsumerId}", id);

            var consumer = await _consumerService.GetConsumerAccountByIdAsync(id);
            if (consumer == null)
            {
                return NotFound(JsonResponse.Error($"Consumer with ID {id} not found"));
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Name))
                consumer.CompanyName = request.Name;
            
            if (!string.IsNullOrEmpty(request.Email))
                consumer.PrimaryContact.Email = request.Email;
            
            if (request.Company != null)
                consumer.CompanyName = request.Company;
            
            if (request.Phone != null)
                consumer.PrimaryContact.Phone = request.Phone;
            
            if (request.Address != null)
                consumer.Address.Street = request.Address;
            
            if (request.City != null)
                consumer.Address.City = request.City;
            
            if (request.Country != null)
                consumer.Address.Country = request.Country;
            
            if (request.PostalCode != null)
                consumer.Address.PostalCode = request.PostalCode;
            
            if (request.Status.HasValue)
                consumer.Status = request.Status.Value;
             

            consumer.Audit.UpdatedBy = GetCurrentUserId();
            consumer.Audit.UpdatedOn = DateTime.UtcNow;

            var updatedConsumer = await _consumerService.UpdateConsumerAccountAsync(consumer, CurrentUserName);
            var response = MapToConsumerResponse(updatedConsumer);

            return Ok(JsonResponse.OK(response, "Consumer updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer {ConsumerId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to update consumer"));
        }
    }

    /// <summary>
    /// Delete a consumer
    /// </summary>
    /// <param name="id">Consumer ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> DeleteConsumer(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting consumer {ConsumerId}", id);

            var consumer = await _consumerService.GetConsumerAccountByIdAsync(id);
            if (consumer == null)
            {
                return NotFound(JsonResponse.Error($"Consumer with ID {id} not found"));
            }

            await _consumerService.DeleteConsumerAccountAsync(id, CurrentUserName);

            return Ok(JsonResponse.OK(null, "Consumer deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer {ConsumerId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to delete consumer"));
        }
    }

    /// <summary>
    /// Get consumer licenses
    /// </summary>
    /// <param name="id">Consumer ID</param>
    /// <returns>Consumer licenses</returns>
    [HttpGet("{id:guid}/licenses")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetConsumerLicenses(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting licenses for consumer {ConsumerId}", id);

            var consumer = await _consumerService.GetConsumerAccountByIdAsync(id);
            if (consumer == null)
            {
                return NotFound(JsonResponse.Error($"Consumer with ID {id} not found"));
            }

            var licenses = await _licenseService.GetLicensesByConsumerAsync(id);
            var response = new GetConsumerLicensesResponse
            {
                Licenses = licenses.Select(license => new ConsumerLicenseResponse
                {
                    Id = license.Id,
                    LicenseKey = license.LicenseKey,
                    ProductName = license.LicenseConsumer.Product?.Name ?? "",
                    ProductVersion = license.LicenseConsumer.Product?.Version ?? "",
                    CreatedDate = license.Audit.CreatedOn,
                    ExpirationDate = license.ValidTo,
                    Status = license.Status.ToString(),
                    TierName = license.LicenseConsumer.ProductTier?.Name ?? ""
                }).ToList(),
                TotalCount = licenses.Count()
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting licenses for consumer {ConsumerId}", id);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve consumer licenses"));
        }
    }

    private ConsumerResponse MapToConsumerResponse(ConsumerAccount consumer)
    {
        var licenses = _licenseService.GetLicensesByConsumerAsync(consumer.ConsumerId).Result;
        return new ConsumerResponse
        {
            Id = consumer.ConsumerId,
            Name = consumer.CompanyName,
            Email = consumer.PrimaryContact?.Email ?? "",
            Company = consumer.CompanyName,
            Phone = consumer.PrimaryContact?.Phone,
            Address = consumer.Address?.Street,
            City = consumer.Address?.City,
            Country = consumer.Address?.Country,
            PostalCode = consumer.Address?.PostalCode,
            Status = consumer.Status,
            CreatedDate = consumer.Audit.CreatedOn,
            ModifiedDate = consumer.Audit.UpdatedOn,
            LicenseCount = licenses?.Count() ?? 0
        };
    }
}
