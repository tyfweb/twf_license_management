using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data; 
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Services.Implementations.Consumer;

/// <summary>
/// Implementation of the Consumer Account service
/// </summary>
public class ConsumerAccountService : IConsumerAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConsumerAccountService> _logger;

    public ConsumerAccountService(
        IUnitOfWork unitOfWork,
        ILogger<ConsumerAccountService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new consumer account
    /// </summary>
    public async Task<ConsumerAccount> CreateConsumerAccountAsync(ConsumerAccount consumerAccount, string createdBy)
    {
        _logger.LogInformation("Creating consumer account for company: {CompanyName}", consumerAccount.CompanyName);

        // Input validation
        if (consumerAccount == null)
            throw new ArgumentNullException(nameof(consumerAccount));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        // Business validation
        var validationResult = await ValidateConsumerAccountAsync(consumerAccount);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Consumer account validation failed: {errors}");
        }

        try
        {
            // Set audit information and ConsumerId if not set
            if (consumerAccount.ConsumerId == Guid.Empty)
            {
                consumerAccount.ConsumerId = Guid.NewGuid();
            }
            
            // Initialize audit fields for creation
            consumerAccount.Audit.CreatedBy = createdBy;
            consumerAccount.Audit.CreatedOn = DateTime.UtcNow;
            consumerAccount.Audit.IsActive = true;
            consumerAccount.Audit.IsDeleted = false;
            
            // Set CreatedAt timestamp if not already set
            if (consumerAccount.CreatedAt == default)
            {
                consumerAccount.CreatedAt = DateTime.UtcNow;
            }
            
            // Ensure PrimaryContact and Address are not null (should be set by constructor, but double-check)
            if (consumerAccount.PrimaryContact == null)
            {
                consumerAccount.PrimaryContact = new ContactPerson();
            }
            
            if (consumerAccount.Address == null)
            {
                consumerAccount.Address = new Address();
            }
            
            // Save to repository using Unit of Work
            var createdEntity = await _unitOfWork.Consumers.AddAsync(consumerAccount); 
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully created consumer account with ID: {ConsumerId}", createdEntity.ConsumerId);
            return createdEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumer account for company: {CompanyName}", consumerAccount.CompanyName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing consumer account
    /// </summary>
    public async Task<ConsumerAccount> UpdateConsumerAccountAsync(ConsumerAccount consumerAccount, string updatedBy)
    {
        _logger.LogInformation("Updating consumer account: {ConsumerId}", consumerAccount.ConsumerId);

        // Input validation
        if (consumerAccount == null)
            throw new ArgumentNullException(nameof(consumerAccount));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));
        if (consumerAccount.ConsumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerAccount.ConsumerId));

        // Check if exists
        var existingEntity = await _unitOfWork.Consumers.GetByIdAsync(consumerAccount.ConsumerId);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Consumer account with ID {consumerAccount.ConsumerId} not found");
        }

        // Business validation
        var validationResult = await ValidateConsumerAccountAsync(consumerAccount);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors);
            throw new InvalidOperationException($"Consumer account validation failed: {errors}");
        }

        try
        {
            existingEntity.CompanyName = consumerAccount.CompanyName;
            existingEntity.PrimaryContact!.Name = consumerAccount.PrimaryContact.Name;
            existingEntity.PrimaryContact!.Email = consumerAccount.PrimaryContact.Email;
            existingEntity.PrimaryContact!.Phone = consumerAccount.PrimaryContact.Phone;
            if (existingEntity.SecondaryContact != null && consumerAccount.SecondaryContact != null)
            {
                existingEntity.SecondaryContact.Name = consumerAccount.SecondaryContact.Name;
                existingEntity.SecondaryContact.Email = consumerAccount.SecondaryContact.Email;
                existingEntity.SecondaryContact.Phone = consumerAccount.SecondaryContact.Phone;
            }
            existingEntity.Address.Street = consumerAccount.Address.Street;
            existingEntity.Address.City = consumerAccount.Address.City;
            existingEntity.Address.State = consumerAccount.Address.State;
            existingEntity.Address.PostalCode = consumerAccount.Address.PostalCode;
            existingEntity.Address.Country = consumerAccount.Address.Country;
            existingEntity.Notes = consumerAccount.Notes;
            existingEntity.Status = consumerAccount.Status;
            existingEntity.Audit.IsActive = consumerAccount.Audit.IsActive;
            
            // Update audit information
            existingEntity.Audit.UpdatedBy = updatedBy;
            existingEntity.Audit.UpdatedOn = DateTime.UtcNow;

            // Update in repository using Unit of Work
            var updatedEntity = await _unitOfWork.Consumers.UpdateAsync(existingEntity.Id, existingEntity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully updated consumer account: {ConsumerId}", updatedEntity.ConsumerId);
            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer account: {ConsumerId}", consumerAccount.ConsumerId);
            throw;
        }
    }

    /// <summary>
    /// Gets a consumer account by ID
    /// </summary>
    public async Task<ConsumerAccount?> GetConsumerAccountByIdAsync(Guid consumerId)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer account by ID: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Gets a consumer account by email address
    /// </summary>
    public async Task<ConsumerAccount?> GetConsumerAccountByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            // TODO: Implement GetByEmailAsync in repository
            _logger.LogWarning("GetConsumerAccountByEmailAsync not fully implemented - repository method GetByEmailAsync missing");
            
            // For now, use search functionality to filter by email
            var searchRequest = new SearchRequest<ConsumerAccount>
            {
                Filters = new Dictionary<string, object>
                {
                    { "PrimaryContactEmail", email },
                    { "SecondaryContactEmail", email }
                }
            };
            
            var searchResult = await _unitOfWork.Consumers.FindOneAsync(searchRequest);
            var entity = searchResult;

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer account by email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gets all consumer accounts with optional filtering
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumerAccountsAsync(
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            // TODO: Implement advanced filtering in repository
            _logger.LogWarning("GetConsumerAccountsAsync using basic search - advanced repository methods missing");
            
            var searchRequest = new SearchRequest<ConsumerAccount>
            {
                Filters = new Dictionary<string, object>()
            };
            // Apply status filter
            if (status.HasValue)
                searchRequest.Filters.Add("Status", status.Value);

            // Apply basic filtering through search
            if (isActive.HasValue)
                searchRequest.Filters.Add("IsActive", isActive.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchRequest.Filters.Add("CompanyName", $"%{searchTerm}%");
            }
            

            searchRequest.Page = pageNumber;
            searchRequest.PageSize = pageSize;
            
            var searchResult = await _unitOfWork.Consumers.SearchAsync(searchRequest);
            return searchResult.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer accounts");
            throw;
        }
    }

    /// <summary>
    /// Gets the total count of consumer accounts with optional filtering
    /// </summary>
    public async Task<int> GetConsumerAccountCountAsync(
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? searchTerm = null)
    {
        try
        {
            // TODO: Implement count methods in repository
            _logger.LogWarning("GetConsumerAccountCountAsync using search for counting - repository count methods missing");
            
            var searchRequest = new SearchRequest<ConsumerAccount>
            {
                Filters = new Dictionary<string, object>(),
                Query=searchTerm
            };
            
            // Apply basic filtering
            if (isActive.HasValue)
                searchRequest.Filters.Add("IsActive", isActive.Value);
             

            var searchResult = await _unitOfWork.Consumers.SearchAsync(searchRequest);
            return searchResult.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer account count");
            throw;
        }
    }

    /// <summary>
    /// Activates a consumer account
    /// </summary>
    public async Task<bool> ActivateConsumerAccountAsync(Guid consumerId, string activatedBy)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(activatedBy))
            throw new ArgumentException("ActivatedBy cannot be null or empty", nameof(activatedBy));

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for activation: {ConsumerId}", consumerId);
                return false;
            } 

            await _unitOfWork.Consumers.ActivateAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully activated consumer account: {ConsumerId}", consumerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating consumer account: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Deactivates a consumer account
    /// </summary>
    public async Task<bool> DeactivateConsumerAccountAsync(Guid consumerId, string deactivatedBy, string reason)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("DeactivatedBy cannot be null or empty", nameof(deactivatedBy));

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for deactivation: {ConsumerId}", consumerId);
                return false;
            }
 
            await _unitOfWork.Consumers.DeleteAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully deactivated consumer account: {ConsumerId}", consumerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating consumer account: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Updates consumer status
    /// </summary>
    public async Task<bool> UpdateConsumerStatusAsync(Guid consumerId, ConsumerStatus status, string updatedBy)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for status update: {ConsumerId}", consumerId);
                return false;
            }

            // TODO: Map status to entity when Status property exists
            _logger.LogWarning("Status update not fully implemented - Status property missing in entity");
             

            await _unitOfWork.Consumers.UpdateAsync(entity.Id, entity);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully updated consumer status: {ConsumerId} to {Status}", consumerId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer status: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a consumer account
    /// </summary>
    public async Task<bool> DeleteConsumerAccountAsync(Guid consumerId, string deletedBy)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for deletion: {ConsumerId}", consumerId);
                return false;
            }

            // TODO: Check for related licenses before deletion
            _logger.LogWarning("Delete operation should check for related licenses first");

            await _unitOfWork.Consumers.DeleteAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully deleted consumer account: {ConsumerId}", consumerId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer account: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Checks if a consumer account exists
    /// </summary>
    public async Task<bool> ConsumerAccountExistsAsync(Guid consumerId)
    {
        if (consumerId == Guid.Empty)
            return false;

        try
        {
            var entity = await _unitOfWork.Consumers.GetByIdAsync(consumerId);
            return entity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if consumer account exists: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Validates consumer account data
    /// </summary>
    public async Task<ValidationResult> ValidateConsumerAccountAsync(ConsumerAccount consumerAccount)
    {
        var errors = new List<string>();

        if (consumerAccount == null)
        {
            errors.Add("Consumer account cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }

        // Required field validations
        if (string.IsNullOrWhiteSpace(consumerAccount.CompanyName))
            errors.Add("Company name is required");

        if (string.IsNullOrWhiteSpace(consumerAccount.PrimaryContact?.Name))
            errors.Add("Primary contact name is required");

        if (string.IsNullOrWhiteSpace(consumerAccount.PrimaryContact?.Email))
            errors.Add("Primary contact email is required");
        else if (!IsValidEmail(consumerAccount.PrimaryContact.Email))
            errors.Add("Primary contact email is not valid");

        // Business rule validations
        if (consumerAccount.ConsumerId != Guid.Empty)
        {
            // Check for duplicate consumer ID (for create operations, this should be auto-generated)
            try
            {
                var existingConsumer = await GetConsumerAccountByIdAsync(consumerAccount.ConsumerId);
                if (existingConsumer != null)
                {
                    errors.Add($"Consumer with ID {consumerAccount.ConsumerId} already exists");
                }
            }
            catch
            {
                // Ignore errors during validation lookup
            }
        }

        // Secondary contact validation (if provided)
        if (consumerAccount.SecondaryContact != null && 
            !string.IsNullOrWhiteSpace(consumerAccount.SecondaryContact.Email) &&
            !IsValidEmail(consumerAccount.SecondaryContact.Email))
        {
            errors.Add("Secondary contact email is not valid");
        }

        return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
    }

    #region Complex Methods - TODO Implementation

    /// <summary>
    /// Gets consumer accounts managed by a specific account manager
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAsync(
        Guid accountManagerId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement when ProductConsumer/AccountManager relationship is available
        _logger.LogWarning("GetConsumersByAccountManagerAsync not implemented - requires ProductConsumer entity and relationships");
        await Task.CompletedTask;
        return Enumerable.Empty<ConsumerAccount>();
    }

    /// <summary>
    /// Gets the count of consumer accounts managed by a specific account manager
    /// </summary>
    public async Task<int> GetConsumerCountByAccountManagerAsync(
        Guid accountManagerId,
        ConsumerStatus? status = null,
        bool? isActive = null)
    {
        // TODO: Implement when ProductConsumer/AccountManager relationship is available
        _logger.LogWarning("GetConsumerCountByAccountManagerAsync not implemented - requires ProductConsumer entity and relationships");
        await Task.CompletedTask;
        return 0;
    }

    /// <summary>
    /// Gets consumer accounts that have licenses for a specific product
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumersByProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement when License repository is available with joins
        _logger.LogWarning("GetConsumersByProductAsync not implemented - requires License repository with consumer joins");
        await Task.CompletedTask;
        return Enumerable.Empty<ConsumerAccount>();
    }

    /// <summary>
    /// Gets the count of consumer accounts that have licenses for a specific product
    /// </summary>
    public async Task<int> GetConsumerCountByProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null)
    {
        // TODO: Implement when License repository is available with joins
        _logger.LogWarning("GetConsumerCountByProductAsync not implemented - requires License repository with consumer joins");
        await Task.CompletedTask;
        return 0;
    }

    /// <summary>
    /// Gets consumer accounts linked to both a specific account manager and product
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAndProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement when all relationships are available
        _logger.LogWarning("GetConsumersByAccountManagerAndProductAsync not implemented - requires complex entity relationships");
        await Task.CompletedTask;
        return Enumerable.Empty<ConsumerAccount>();
    }

    /// <summary>
    /// Gets the count of consumer accounts linked to both a specific account manager and product
    /// </summary>
    public async Task<int> GetConsumerCountByAccountManagerAndProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        string? licenseStatus = null)
    {
        // TODO: Implement when all relationships are available
        _logger.LogWarning("GetConsumerCountByAccountManagerAndProductAsync not implemented - requires complex entity relationships");
        await Task.CompletedTask;
        return 0;
    }

    /// <summary>
    /// Gets consumer accounts that don't have any licenses for a specific product
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumersWithoutProductAsync(
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement when License repository is available with complex queries
        _logger.LogWarning("GetConsumersWithoutProductAsync not implemented - requires License repository with NOT EXISTS queries");
        await Task.CompletedTask;
        return Enumerable.Empty<ConsumerAccount>();
    }

    /// <summary>
    /// Gets consumer accounts managed by an account manager that don't have licenses for a specific product
    /// </summary>
    public async Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerWithoutProductAsync(
        Guid accountManagerId,
        Guid productId,
        ConsumerStatus? status = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement when all relationships and complex queries are available
        _logger.LogWarning("GetConsumersByAccountManagerWithoutProductAsync not implemented - requires complex entity relationships and queries");
        await Task.CompletedTask;
        return Enumerable.Empty<ConsumerAccount>();
    }

    #endregion

    #region Consumer Contact Management (Addon Feature)

    /// <summary>
    /// Creates a new consumer contact
    /// </summary>
    public async Task<ConsumerContact> CreateConsumerContactAsync(ConsumerContact consumerContact, string createdBy)
    {
        _logger.LogInformation("Creating consumer contact for consumer: {ConsumerId}, Contact: {ContactName}", 
            consumerContact.ConsumerId, consumerContact.ContactName);

        // Input validation
        if (consumerContact == null)
            throw new ArgumentNullException(nameof(consumerContact));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        // Business validation
        if (string.IsNullOrWhiteSpace(consumerContact.ContactName))
            throw new ArgumentException("Contact name is required", nameof(consumerContact.ContactName));
        if (string.IsNullOrWhiteSpace(consumerContact.ContactEmail))
            throw new ArgumentException("Contact email is required", nameof(consumerContact.ContactEmail));
        if (!IsValidEmail(consumerContact.ContactEmail))
            throw new ArgumentException("Invalid email format", nameof(consumerContact.ContactEmail));

        // Verify consumer exists
        var consumer = await _unitOfWork.Consumers.GetByIdAsync(consumerContact.ConsumerId);
        if (consumer == null)
            throw new InvalidOperationException($"Consumer with ID {consumerContact.ConsumerId} not found");

        try
        {
            // Set audit information
            consumerContact.Audit.CreatedBy = createdBy;
            consumerContact.Audit.CreatedOn = DateTime.UtcNow;
            consumerContact.Audit.IsActive = true;
            consumerContact.Audit.IsDeleted = false;

            // Generate new ID if not provided
            if (consumerContact.ContactId == Guid.Empty)
                consumerContact.ContactId = Guid.NewGuid();

            // Add contact to repository
            await _unitOfWork.ConsumerContacts.AddAsync(consumerContact);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created consumer contact: {ContactId} for consumer: {ConsumerId}", 
                consumerContact.ContactId, consumerContact.ConsumerId);
            
            return consumerContact;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consumer contact for consumer: {ConsumerId}", consumerContact.ConsumerId);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing consumer contact
    /// </summary>
    public async Task<ConsumerContact> UpdateConsumerContactAsync(ConsumerContact consumerContact, string updatedBy)
    {
        _logger.LogInformation("Updating consumer contact: {ContactId}", consumerContact.ContactId);

        // Input validation
        if (consumerContact == null)
            throw new ArgumentNullException(nameof(consumerContact));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

        // Business validation
        if (string.IsNullOrWhiteSpace(consumerContact.ContactName))
            throw new ArgumentException("Contact name is required", nameof(consumerContact.ContactName));
        if (string.IsNullOrWhiteSpace(consumerContact.ContactEmail))
            throw new ArgumentException("Contact email is required", nameof(consumerContact.ContactEmail));
        if (!IsValidEmail(consumerContact.ContactEmail))
            throw new ArgumentException("Invalid email format", nameof(consumerContact.ContactEmail));

        try
        {
            // Update audit information
            consumerContact.Audit.UpdatedBy = updatedBy;
            consumerContact.Audit.UpdatedOn = DateTime.UtcNow;

            // Update contact in repository
            await _unitOfWork.ConsumerContacts.UpdateAsync(consumerContact.ContactId, consumerContact);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated consumer contact: {ContactId}", consumerContact.ContactId);
            
            return consumerContact;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consumer contact: {ContactId}", consumerContact.ContactId);
            throw;
        }
    }

    /// <summary>
    /// Gets a consumer contact by ID
    /// </summary>
    public async Task<ConsumerContact?> GetConsumerContactByIdAsync(Guid contactId)
    {
        _logger.LogInformation("Getting consumer contact by ID: {ContactId}", contactId);

        try
        {
            var contact = await _unitOfWork.ConsumerContacts.GetByIdAsync(contactId);
            
            _logger.LogInformation("Successfully retrieved consumer contact: {ContactId}", contactId);
            return contact;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer contact by ID: {ContactId}", contactId);
            throw;
        }
    }

    /// <summary>
    /// Gets all contacts for a specific consumer
    /// </summary>
    public async Task<IEnumerable<ConsumerContact>> GetConsumerContactsByConsumerIdAsync(Guid consumerId)
    {
        _logger.LogInformation("Getting consumer contacts for consumer: {ConsumerId}", consumerId);

        try
        {
            // Use the optimized repository method to get contacts by consumer ID
            var consumerContacts = await _unitOfWork.ConsumerContacts.GetByConsumerIdAsync(consumerId);
            
            _logger.LogInformation("Successfully retrieved {Count} consumer contacts for consumer: {ConsumerId}", 
                consumerContacts.Count(), consumerId);
            
            return consumerContacts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer contacts for consumer: {ConsumerId}", consumerId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a consumer contact
    /// </summary>
    public async Task<bool> DeleteConsumerContactAsync(Guid contactId, string deletedBy)
    {
        _logger.LogInformation("Deleting consumer contact: {ContactId}", contactId);

        // Input validation
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        try
        {
            // Delete the contact from repository
            var result = await _unitOfWork.ConsumerContacts.DeleteAsync(contactId);
            
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted consumer contact: {ContactId}", contactId);
            }
            else
            {
                _logger.LogWarning("Consumer contact not found for deletion: {ContactId}", contactId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer contact: {ContactId}", contactId);
            throw;
        }
    }

    /// <summary>
    /// Sets a contact as primary for a specific consumer
    /// </summary>
    public async Task<bool> SetPrimaryConsumerContactAsync(Guid contactId, string updatedBy)
    {
        _logger.LogInformation("Setting consumer contact as primary: {ContactId}", contactId);

        // Input validation
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

        try
        {
            // First, get the contact to set as primary
            var contactToSetPrimary = await _unitOfWork.ConsumerContacts.GetByIdAsync(contactId);
            if (contactToSetPrimary == null)
            {
                _logger.LogWarning("Contact not found: {ContactId}", contactId);
                return false;
            }

            // Get all contacts for this consumer
            var allContacts = await GetConsumerContactsByConsumerIdAsync(contactToSetPrimary.ConsumerId);
            
            // Update all contacts: set the target as primary, others as non-primary
            foreach (var contact in allContacts)
            {
                var wasChanged = false;
                
                if (contact.ContactId == contactId)
                {
                    // Set this contact as primary
                    if (!contact.IsPrimaryContact)
                    {
                        contact.IsPrimaryContact = true;
                        contact.Audit.UpdatedBy = updatedBy;
                        contact.Audit.UpdatedOn = DateTime.UtcNow;
                        wasChanged = true;
                    }
                }
                else
                {
                    // Unset other contacts as primary
                    if (contact.IsPrimaryContact)
                    {
                        contact.IsPrimaryContact = false;
                        contact.Audit.UpdatedBy = updatedBy;
                        contact.Audit.UpdatedOn = DateTime.UtcNow;
                        wasChanged = true;
                    }
                }

                // Update if changed
                if (wasChanged)
                {
                    await _unitOfWork.ConsumerContacts.UpdateAsync(contact.ContactId, contact);
                }
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully set consumer contact as primary: {ContactId} for consumer: {ConsumerId}", 
                contactId, contactToSetPrimary.ConsumerId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting consumer contact as primary: {ContactId}", contactId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
