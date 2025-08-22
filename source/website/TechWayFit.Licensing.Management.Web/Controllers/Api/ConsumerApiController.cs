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
[Route("api/consumer")]
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

    #region Consumer Contact Management (Addon Feature)

    /// <summary>
    /// Get all contacts for a specific consumer
    /// </summary>
    /// <param name="consumerId">Consumer ID</param>
    /// <returns>List of consumer contacts</returns>
    [HttpGet("{consumerId:guid}/contacts")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetConsumerContacts(Guid consumerId)
    {
        try
        {
            _logger.LogInformation("Getting contacts for consumer: {ConsumerId}", consumerId);

            // Verify consumer exists
            var consumer = await _consumerService.GetConsumerAccountByIdAsync(consumerId);
            if (consumer == null)
            {
                return NotFound(JsonResponse.Error("Consumer not found"));
            }

            var contacts = await _consumerService.GetConsumerContactsByConsumerIdAsync(consumerId);
            var response = contacts.Select(contact => new
            {
                Id = contact.ContactId,
                ConsumerId = contact.ConsumerId,
                ContactName = contact.ContactName,
                ContactEmail = contact.ContactEmail,
                ContactPhone = contact.ContactPhone,
                ContactAddress = contact.ContactAddress,
                CompanyDivision = contact.CompanyDivision,
                ContactDesignation = contact.ContactDesignation,
                IsPrimaryContact = contact.IsPrimaryContact,
                ContactType = contact.ContactType,
                Notes = contact.Notes,
                CreatedDate = contact.Audit.CreatedOn,
                ModifiedDate = contact.Audit.UpdatedOn
            }).ToList();

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts for consumer {ConsumerId}", consumerId);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve consumer contacts"));
        }
    }

    /// <summary>
    /// Get a specific consumer contact by ID
    /// </summary>
    /// <param name="contactId">Contact ID</param>
    /// <returns>Consumer contact details</returns>
    [HttpGet("contacts/{contactId:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> GetConsumerContact(Guid contactId)
    {
        try
        {
            _logger.LogInformation("Getting consumer contact: {ContactId}", contactId);

            var contact = await _consumerService.GetConsumerContactByIdAsync(contactId);
            if (contact == null)
            {
                return NotFound(JsonResponse.Error("Consumer contact not found"));
            }

            var response = new
            {
                Id = contact.ContactId,
                ConsumerId = contact.ConsumerId,
                ContactName = contact.ContactName,
                ContactEmail = contact.ContactEmail,
                ContactPhone = contact.ContactPhone,
                ContactAddress = contact.ContactAddress,
                CompanyDivision = contact.CompanyDivision,
                ContactDesignation = contact.ContactDesignation,
                IsPrimaryContact = contact.IsPrimaryContact,
                ContactType = contact.ContactType,
                Notes = contact.Notes,
                CreatedDate = contact.Audit.CreatedOn,
                ModifiedDate = contact.Audit.UpdatedOn
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer contact {ContactId}", contactId);
            return StatusCode(500, JsonResponse.Error("Failed to retrieve consumer contact"));
        }
    }

    /// <summary>
    /// Create a new consumer contact
    /// </summary>
    /// <param name="request">Consumer contact creation request</param>
    /// <returns>Created consumer contact</returns>
    [HttpPost("contacts")]
    [ProducesResponseType(typeof(JsonResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> CreateConsumerContact([FromBody] CreateConsumerContactRequest request)
    {
        try
        {
            _logger.LogInformation("Creating consumer contact for consumer: {ConsumerId}", request.ConsumerId);

            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            var contact = new ConsumerContact
            {
                ConsumerId = request.ConsumerId,
                TenantId = Guid.Empty, // TODO: Implement tenant context
                ContactName = request.ContactName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone ?? string.Empty,
                ContactAddress = request.ContactAddress ?? string.Empty,
                CompanyDivision = request.CompanyDivision ?? string.Empty,
                ContactDesignation = request.ContactDesignation ?? string.Empty,
                IsPrimaryContact = request.IsPrimaryContact,
                ContactType = request.ContactType ?? string.Empty,
                Notes = request.Notes ?? string.Empty
            };

            var createdContact = await _consumerService.CreateConsumerContactAsync(contact, CurrentUserName);

            var response = new
            {
                Id = createdContact.ContactId,
                ConsumerId = createdContact.ConsumerId,
                ContactName = createdContact.ContactName,
                ContactEmail = createdContact.ContactEmail,
                ContactPhone = createdContact.ContactPhone,
                ContactAddress = createdContact.ContactAddress,
                CompanyDivision = createdContact.CompanyDivision,
                ContactDesignation = createdContact.ContactDesignation,
                IsPrimaryContact = createdContact.IsPrimaryContact,
                ContactType = createdContact.ContactType,
                Notes = createdContact.Notes,
                CreatedDate = createdContact.Audit.CreatedOn
            };

            return CreatedAtAction(nameof(GetConsumerContact), new { contactId = createdContact.ContactId }, JsonResponse.OK(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid consumer contact data");
            return BadRequest(JsonResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumer contact for consumer: {ConsumerId}", request.ConsumerId);
            return StatusCode(500, JsonResponse.Error("Failed to create consumer contact"));
        }
    }

    /// <summary>
    /// Update an existing consumer contact
    /// </summary>
    /// <param name="contactId">Contact ID</param>
    /// <param name="request">Consumer contact update request</param>
    /// <returns>Updated consumer contact</returns>
    [HttpPut("contacts/{contactId:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> UpdateConsumerContact(Guid contactId, [FromBody] UpdateConsumerContactRequest request)
    {
        try
        {
            _logger.LogInformation("Updating consumer contact: {ContactId}", contactId);

            if (!ModelState.IsValid)
            {
                return BadRequest(JsonResponse.Error("Invalid request data"));
            }

            var existingContact = await _consumerService.GetConsumerContactByIdAsync(contactId);
            if (existingContact == null)
            {
                return NotFound(JsonResponse.Error("Consumer contact not found"));
            }

            existingContact.ContactName = request.ContactName;
            existingContact.ContactEmail = request.ContactEmail;
            existingContact.ContactPhone = request.ContactPhone ?? string.Empty;
            existingContact.ContactAddress = request.ContactAddress ?? string.Empty;
            existingContact.CompanyDivision = request.CompanyDivision ?? string.Empty;
            existingContact.ContactDesignation = request.ContactDesignation ?? string.Empty;
            existingContact.IsPrimaryContact = request.IsPrimaryContact;
            existingContact.ContactType = request.ContactType ?? string.Empty;
            existingContact.Notes = request.Notes ?? string.Empty;

            var updatedContact = await _consumerService.UpdateConsumerContactAsync(existingContact, CurrentUserName);

            var response = new
            {
                Id = updatedContact.ContactId,
                ConsumerId = updatedContact.ConsumerId,
                ContactName = updatedContact.ContactName,
                ContactEmail = updatedContact.ContactEmail,
                ContactPhone = updatedContact.ContactPhone,
                ContactAddress = updatedContact.ContactAddress,
                CompanyDivision = updatedContact.CompanyDivision,
                ContactDesignation = updatedContact.ContactDesignation,
                IsPrimaryContact = updatedContact.IsPrimaryContact,
                ContactType = updatedContact.ContactType,
                Notes = updatedContact.Notes,
                ModifiedDate = updatedContact.Audit.UpdatedOn
            };

            return Ok(JsonResponse.OK(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid consumer contact data");
            return BadRequest(JsonResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer contact: {ContactId}", contactId);
            return StatusCode(500, JsonResponse.Error("Failed to update consumer contact"));
        }
    }

    /// <summary>
    /// Delete a consumer contact
    /// </summary>
    /// <param name="contactId">Contact ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("contacts/{contactId:guid}")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> DeleteConsumerContact(Guid contactId)
    {
        try
        {
            _logger.LogInformation("Deleting consumer contact: {ContactId}", contactId);

            var contact = await _consumerService.GetConsumerContactByIdAsync(contactId);
            if (contact == null)
            {
                return NotFound(JsonResponse.Error("Consumer contact not found"));
            }

            var deleted = await _consumerService.DeleteConsumerContactAsync(contactId, CurrentUserName);
            if (!deleted)
            {
                return StatusCode(500, JsonResponse.Error("Failed to delete consumer contact"));
            }

            return Ok(JsonResponse.OK("Consumer contact deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer contact: {ContactId}", contactId);
            return StatusCode(500, JsonResponse.Error("Failed to delete consumer contact"));
        }
    }

    /// <summary>
    /// Set a consumer contact as primary
    /// </summary>
    /// <param name="contactId">Contact ID</param>
    /// <returns>Success response</returns>
    [HttpPut("contacts/{contactId:guid}/set-primary")]
    [ProducesResponseType(typeof(JsonResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<JsonResponse>> SetPrimaryConsumerContact(Guid contactId)
    {
        try
        {
            _logger.LogInformation("Setting consumer contact as primary: {ContactId}", contactId);

            var contact = await _consumerService.GetConsumerContactByIdAsync(contactId);
            if (contact == null)
            {
                return NotFound(JsonResponse.Error("Consumer contact not found"));
            }

            var updated = await _consumerService.SetPrimaryConsumerContactAsync(contactId, CurrentUserName);
            if (!updated)
            {
                return StatusCode(500, JsonResponse.Error("Failed to set consumer contact as primary"));
            }

            return Ok(JsonResponse.OK("Consumer contact set as primary successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting consumer contact as primary: {ContactId}", contactId);
            return StatusCode(500, JsonResponse.Error("Failed to set consumer contact as primary"));
        }
    }

    #endregion
}
