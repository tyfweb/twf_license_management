using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Services.Implementations.Consumer;

/// <summary>
/// Implementation of the Consumer Account service
/// </summary>
public class ConsumerAccountService : IConsumerAccountService
{
    private readonly IConsumerAccountRepository _consumerAccountRepository;
    private readonly ILogger<ConsumerAccountService> _logger;

    public ConsumerAccountService(
        IConsumerAccountRepository consumerAccountRepository,
        ILogger<ConsumerAccountService> logger)
    {
        _consumerAccountRepository = consumerAccountRepository ?? throw new ArgumentNullException(nameof(consumerAccountRepository));
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
            // Map to entity
            var entity = ConsumerAccountEntity.FromModel(consumerAccount);
            entity.CreatedBy = createdBy;
            entity.CreatedOn = DateTime.UtcNow;
            entity.UpdatedBy = createdBy;
            entity.UpdatedOn = DateTime.UtcNow;

            // Save to repository
            var createdEntity = await _consumerAccountRepository.AddAsync(entity);
            
            // Map back to model
            var result = createdEntity.ToModel();
            
            _logger.LogInformation("Successfully created consumer account with ID: {ConsumerId}", result.ConsumerId);
            return result;
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
        if (string.IsNullOrWhiteSpace(consumerAccount.ConsumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerAccount.ConsumerId));

        // Check if exists
        var existingEntity = await _consumerAccountRepository.GetByIdAsync(consumerAccount.ConsumerId);
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
            // Map updates to existing entity
            var updatedData = ConsumerAccountEntity.FromModel(consumerAccount);
            
            // Update properties manually to preserve audit fields
            existingEntity.CompanyName = updatedData.CompanyName;
            existingEntity.PrimaryContactName = updatedData.PrimaryContactName;
            existingEntity.PrimaryContactEmail = updatedData.PrimaryContactEmail;
            existingEntity.PrimaryContactPhone = updatedData.PrimaryContactPhone;
            existingEntity.SecondaryContactName = updatedData.SecondaryContactName;
            existingEntity.SecondaryContactEmail = updatedData.SecondaryContactEmail;
            existingEntity.SecondaryContactPhone = updatedData.SecondaryContactPhone;
            existingEntity.AddressStreet = updatedData.AddressStreet;
            existingEntity.AddressCity = updatedData.AddressCity;
            existingEntity.AddressState = updatedData.AddressState;
            existingEntity.AddressPostalCode = updatedData.AddressPostalCode;
            existingEntity.AddressCountry = updatedData.AddressCountry;
            existingEntity.Notes = updatedData.Notes;
            existingEntity.Status = updatedData.Status;
            existingEntity.IsActive = updatedData.IsActive;
            
            existingEntity.UpdatedBy = updatedBy;
            existingEntity.UpdatedOn = DateTime.UtcNow;

            // Update in repository
            var updatedEntity = await _consumerAccountRepository.UpdateAsync(existingEntity);
            
            // Map back to model
            var result = updatedEntity.ToModel();
            
            _logger.LogInformation("Successfully updated consumer account: {ConsumerId}", result.ConsumerId);
            return result;
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
    public async Task<ConsumerAccount?> GetConsumerAccountByIdAsync(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerId));

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
            return entity?.ToModel();
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
            var searchRequest = new SearchRequest<ConsumerAccountEntity>
            {
                Filters = new List<Expression<Func<ConsumerAccountEntity, bool>>>
                {
                    c => c.PrimaryContactEmail.Equals(email, StringComparison.OrdinalIgnoreCase) ||
                         (c.SecondaryContactEmail != null && c.SecondaryContactEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                }
            };
            
            var searchResult = await _consumerAccountRepository.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();
            
            return entity?.ToModel();
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
            
            var searchRequest = new SearchRequest<ConsumerAccountEntity>
            {
                Filters = new List<Expression<Func<ConsumerAccountEntity, bool>>>()
            };
            
            // Apply basic filtering through search
            if (isActive.HasValue)
                searchRequest.Filters.Add(e => e.IsActive == isActive.Value);
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
               
                searchRequest.Filters.Add(e => 
                    EF.Functions.Like(e.CompanyName, $"%{searchTerm}%"));
            }

            searchRequest.Page = pageNumber;
            searchRequest.PageSize = pageSize;
            
            var searchResult = await _consumerAccountRepository.SearchAsync(searchRequest);
            return searchResult.Results.Select(e => e.ToModel());
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
            
            var searchRequest = new SearchRequest<ConsumerAccountEntity>
            {
                Filters = new List<Expression<Func<ConsumerAccountEntity, bool>>>()
            };
            
            // Apply basic filtering
            if (isActive.HasValue)
                searchRequest.Filters.Add(e => e.IsActive == isActive.Value);
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchRequest.Filters.Add(e => 
                     EF.Functions.Like(e.CompanyName, $"%{searchTerm}%"));
            }

            var searchResult = await _consumerAccountRepository.SearchAsync(searchRequest);
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
    public async Task<bool> ActivateConsumerAccountAsync(string consumerId, string activatedBy)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(activatedBy))
            throw new ArgumentException("ActivatedBy cannot be null or empty", nameof(activatedBy));

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for activation: {ConsumerId}", consumerId);
                return false;
            }

            entity.IsActive = true;
            entity.ActivatedAt = DateTime.UtcNow;
            entity.UpdatedBy = activatedBy;
            entity.UpdatedOn = DateTime.UtcNow;

            await _consumerAccountRepository.UpdateAsync(entity);
            
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
    public async Task<bool> DeactivateConsumerAccountAsync(string consumerId, string deactivatedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(deactivatedBy))
            throw new ArgumentException("DeactivatedBy cannot be null or empty", nameof(deactivatedBy));

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for deactivation: {ConsumerId}", consumerId);
                return false;
            }

            entity.IsActive = false;
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes) 
                ? $"Deactivated on {DateTime.UtcNow:yyyy-MM-dd}: {reason}"
                : $"{entity.Notes}\nDeactivated on {DateTime.UtcNow:yyyy-MM-dd}: {reason}";
            entity.UpdatedBy = deactivatedBy;
            entity.UpdatedOn = DateTime.UtcNow;

            await _consumerAccountRepository.UpdateAsync(entity);
            
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
    public async Task<bool> UpdateConsumerStatusAsync(string consumerId, ConsumerStatus status, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for status update: {ConsumerId}", consumerId);
                return false;
            }

            // TODO: Map status to entity when Status property exists
            _logger.LogWarning("Status update not fully implemented - Status property missing in entity");
            
            entity.UpdatedBy = updatedBy;
            entity.UpdatedOn = DateTime.UtcNow;

            await _consumerAccountRepository.UpdateAsync(entity);
            
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
    public async Task<bool> DeleteConsumerAccountAsync(string consumerId, string deletedBy)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            throw new ArgumentException("ConsumerId cannot be null or empty", nameof(consumerId));
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
            if (entity == null)
            {
                _logger.LogWarning("Consumer account not found for deletion: {ConsumerId}", consumerId);
                return false;
            }

            // TODO: Check for related licenses before deletion
            _logger.LogWarning("Delete operation should check for related licenses first");

            await _consumerAccountRepository.DeleteAsync(entity.ConsumerId);
            
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
    public async Task<bool> ConsumerAccountExistsAsync(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId))
            return false;

        try
        {
            var entity = await _consumerAccountRepository.GetByIdAsync(consumerId);
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
        if (!string.IsNullOrWhiteSpace(consumerAccount.ConsumerId))
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
        string accountManagerId,
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
        string accountManagerId,
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
        string productId,
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
        string productId,
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
        string accountManagerId,
        string productId,
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
        string accountManagerId,
        string productId,
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
        string productId,
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
        string accountManagerId,
        string productId,
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
