using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TechWayFit.Licensing.Generator.Models;
using TechWayFit.Licensing.Generator.Services;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Services.Factories;
using System.Text.Json;
using CoreModels = TechWayFit.Licensing.Core.Models;
using ManagementModels = TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Services.Implementations.License;

/// <summary>
/// Implementation of the Product License service
/// </summary>
public class ProductLicenseService : IProductLicenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILicenseGenerationFactory _licenseGenerationFactory;
    private readonly ILogger<ProductLicenseService> _logger;

    public ProductLicenseService(
        IUnitOfWork unitOfWork,
        ILicenseGenerationFactory licenseGenerationFactory,
        ILogger<ProductLicenseService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _licenseGenerationFactory = licenseGenerationFactory ?? throw new ArgumentNullException(nameof(licenseGenerationFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates a new product license using the appropriate strategy pattern
    /// </summary>
    public async Task<ProductLicense> GenerateLicenseAsync(LicenseGenerationRequest request, string generatedBy)
    {
        _logger.LogInformation("Generating {LicenseModel} license for product: {ProductId}, consumer: {ConsumerId}",
            request.LicenseModel, request.ProductId, request.ConsumerId);

        try
        {
            // Use the factory to generate the license with the appropriate strategy
            var result = await _licenseGenerationFactory.GenerateAsync(request, generatedBy);

            _logger.LogInformation("Successfully generated {LicenseModel} license with ID: {LicenseId} and key: {LicenseKey}", 
                request.LicenseModel, result.LicenseId, result.LicenseKey);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {LicenseModel} license for product: {ProductId}", 
                request.LicenseModel, request.ProductId);
            throw;
        }
    }

    /// <summary>
    /// Gets a license by license key
    /// </summary>
    public async Task<ProductLicense?> GetLicenseByKeyAsync(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));

        try
        {
            // TODO: Implement GetByLicenseKeyAsync in repository
            _logger.LogWarning("GetLicenseByKeyAsync using search - GetByLicenseKeyAsync repository method missing");

            var searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>
                {
                    { nameof(ProductLicense.LicenseKey), licenseKey }
                }
            };

            var searchResult = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            var entity = searchResult.Results.FirstOrDefault();

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license by key: {LicenseKey}", licenseKey);
            throw;
        }
    }

    /// <summary>
    /// Validates a license
    /// </summary>
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, Guid productId, bool checkActivation = true)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey cannot be null or empty", nameof(licenseKey));
        if (Guid.Empty.Equals(productId))
            throw new ArgumentException("ProductId cannot be null or empty", nameof(productId));

        try
        {
            var license = await GetLicenseByKeyAsync(licenseKey);

            if (license == null)
            {
                return LicenseValidationResult.Failure(LicenseStatus.NotFound, "License not found");
            }

            // Check product match
            if (license.LicenseConsumer?.Product?.ProductId != productId)
            {
                return LicenseValidationResult.Failure(LicenseStatus.NotFound,
                "License does not match the specified product");
            }

            // Check license status and validity
            var now = DateTime.UtcNow;
            if (license.Status != LicenseStatus.Active || now < license.ValidFrom || now > license.ValidTo)
            {
                return LicenseValidationResult.Failure(LicenseStatus.Expired,
                    "License is not active or is outside the valid date range");
            }

            // License is valid
            return LicenseValidationResult.Success(license.ToLicenseModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license: {LicenseKey}", licenseKey);

            return LicenseValidationResult.Failure(LicenseStatus.ServiceUnavailable, 
                    "An error occurred while validating the license. Please try again later.");
        }
    }

    #region TODO: Missing Interface Methods - Require Implementation

    public async Task<ProductLicense> UpdateLicenseAsync(Guid licenseId, LicenseUpdateRequest request, string updatedBy)
    {
        // TODO: Implement when entity properties are available
        _logger.LogWarning("UpdateLicenseAsync not fully implemented - entity properties missing");
        await Task.CompletedTask;
        throw new NotImplementedException("UpdateLicenseAsync not implemented - entity structure incomplete");
    }

    public async Task<ProductLicense> RegenerateLicenseKeyAsync(Guid licenseId, string regeneratedBy, string reason)
    {
        // TODO: Implement
        _logger.LogWarning("RegenerateLicenseKeyAsync not implemented");
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<ProductLicense?> GetLicenseByIdAsync(Guid licenseId)
    {
        var response = await _unitOfWork.Licenses.GetByIdWithAllIncludesAsync(licenseId);
        if (response == null)
        {
            _logger.LogWarning("License with ID {LicenseId} not found", licenseId);
            return null;
        }
        return response;
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByConsumerAsync(Guid consumerId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        var response = await _unitOfWork.Licenses.GetByConsumerIdAsync(consumerId);
        if (response == null || !response.Any())
        {
            return Enumerable.Empty<ProductLicense>();
        } 

        return response
            .Where(l => !status.HasValue || l.Status == status.Value)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesByProductAsync(Guid productId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>
                {
                    { nameof(ProductLicense.ProductId), productId }
                },
                Page = pageNumber,
                PageSize = pageSize
            };

            if (status.HasValue)
            {
                searchRequest.Filters.Add("Status", status.Value.ToString());
            }

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            return result.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting licenses for product: {ProductId}", productId);
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysAhead = 30)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysAhead);
            
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>
                {
                    { "Status", LicenseStatus.Active.ToString() }
                },
                Page = 1,
                PageSize = 1000 // Get a large number of expiring licenses
            };

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            
            // Filter by expiry date in memory since complex date filtering might not be supported by search
            return result.Results.Where(l => 
                l.ValidTo <= expiryDate && 
                l.ValidTo > DateTime.UtcNow && 
                l.Status == LicenseStatus.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring licenses for {DaysAhead} days", daysAhead);
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetExpiredLicensesAsync()
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>(),
                Page = 1,
                PageSize = 1000 // Get a large number of licenses
            };

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            
            // Filter expired licenses in memory
            return result.Results.Where(l => 
                l.ValidTo <= DateTime.UtcNow && 
                (l.Status == LicenseStatus.Active || l.Status == LicenseStatus.Expired));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired licenses");
            return Enumerable.Empty<ProductLicense>();
        }
    }

    public async Task<bool> ActivateLicenseAsync(Guid licenseId, ActivationInfo activationInfo)
    {
        // TODO: Implement
        _logger.LogWarning("ActivateLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeactivateLicenseAsync(Guid licenseId, string deactivatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("DeactivateLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> SuspendLicenseAsync(Guid licenseId, string suspendedBy, string reason, DateTime? suspendUntil = null)
    {
        // TODO: Implement
        _logger.LogWarning("SuspendLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> RevokeLicenseAsync(Guid licenseId, string revokedBy, string reason)
    {
        // TODO: Implement
        _logger.LogWarning("RevokeLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> RenewLicenseAsync(Guid licenseId, DateTime newExpiryDate, string renewedBy)
    {
        // TODO: Implement
        _logger.LogWarning("RenewLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> UpdateLicenseStatusAsync(Guid licenseId, LicenseStatus status, string updatedBy, string? reason = null)
    {
        // TODO: Implement
        _logger.LogWarning("UpdateLicenseStatusAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeleteLicenseAsync(Guid licenseId, string deletedBy)
    {
        // TODO: Implement
        _logger.LogWarning("DeleteLicenseAsync not implemented");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> LicenseExistsAsync(string licenseKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return false;

            var license = await GetLicenseByKeyAsync(licenseKey);
            return license != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if license exists: {LicenseKey}", licenseKey);
            return false;
        }
    }

    public Task<ValidationResult> ValidateLicenseDataAsync(ProductLicense license)
    {
        try
        {
            var errors = new List<string>();

            if (license == null)
            {
                errors.Add("License cannot be null");
                return Task.FromResult(ValidationResult.Failure(errors.ToArray()));
            }

            if (license.ProductId == Guid.Empty)
                errors.Add("License must have a valid ProductId");

            if (license.ConsumerId == Guid.Empty)
                errors.Add("License must have a valid ConsumerId");

            if (string.IsNullOrWhiteSpace(license.LicenseKey))
                errors.Add("License must have a valid LicenseKey");

            if (license.ValidFrom >= license.ValidTo)
                errors.Add("ValidFrom must be before ValidTo");

            return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license data");
            return Task.FromResult(ValidationResult.Failure("An error occurred during license data validation"));
        }
    }

    public Task<ValidationResult> ValidateLicenseGenerationRequestAsync(LicenseGenerationRequest request)
    {
        try
        {
            var errors = new List<string>();

            // Basic validation
            if (request == null)
            {
                errors.Add("License generation request cannot be null");
                return Task.FromResult(ValidationResult.Failure(errors.ToArray()));
            }

            if (request.ProductId == Guid.Empty)
                errors.Add("ProductId is required");

            if (request.ConsumerId == Guid.Empty)
                errors.Add("ConsumerId is required");

            if (request.ExpiryDate.HasValue && request.ExpiryDate <= DateTime.UtcNow)
                errors.Add("Expiry date must be in the future");

            if (request.MaxUsers.HasValue && request.MaxUsers <= 0)
                errors.Add("Max users must be greater than 0");

            if (request.MaxDevices.HasValue && request.MaxDevices <= 0)
                errors.Add("Max devices must be greater than 0");

            return Task.FromResult(errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license generation request");
            return Task.FromResult(ValidationResult.Failure("An error occurred during validation"));
        }
    }

    public async Task<ValidationResult> ValidateLicenseUpdateRequestAsync(Guid licenseId, LicenseUpdateRequest request)
    {
        // TODO: Implement
        _logger.LogWarning("ValidateLicenseUpdateRequestAsync not implemented");
        await Task.CompletedTask;
        return ValidationResult.Success();
    }

    public async Task<IEnumerable<ProductLicense>> GetAllLicensesAsync(LicenseStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        // Delegate to GetLicensesAsync which has the same functionality
        return await GetLicensesAsync(status, searchTerm, pageNumber, pageSize);
    }

    public async Task<int> GetLicenseCountAsync(LicenseStatus? status = null, string? searchTerm = null)
    {
        try
        {
            SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
            {
                Filters = new Dictionary<string, object>(),
                Page = 1,
                PageSize = int.MaxValue, // Get all to count
                Query = searchTerm
            };

            if (status.HasValue)
            {
                searchRequest.Filters.Add("Status", status.Value.ToString());
            }

            var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
            return result.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license count");
            return 0;
        }
    }

    public async Task<LicenseUsageStatistics> GetLicenseUsageStatisticsAsync(Guid? productId = null, Guid? consumerId = null)
    {
        var result = await _unitOfWork.Licenses.GetUsageStatisticsAsync(productId, consumerId);
        _logger.LogWarning("GetLicenseUsageStatisticsAsync not implemented");
        return result;
    }

    public async Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(Guid licenseId)
    {
        // TODO: Implement
        _logger.LogWarning("GetLicenseAuditHistoryAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<LicenseAuditEntry>();
    }

    #endregion

    #region Private Helper Methods

    private static string SerializeMetadata(Dictionary<string, object> metadata)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Serialize(metadata);
        }
        catch
        {
            return "{}";
        }
    }

    public async Task<IEnumerable<ProductLicense>> GetLicensesAsync(LicenseStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        SearchRequest<ProductLicense> searchRequest = new SearchRequest<ProductLicense>
        {
            Filters = new Dictionary<string, object>(),
            Page = pageNumber,
            PageSize = pageSize,
            Query= searchTerm
        };

        if (status.HasValue)
        {
            searchRequest.Filters.Add("Status", status.Value.ToString());
        }



        var result = await _unitOfWork.Licenses.SearchAsync(searchRequest);
        return result.Results;
    }


    #endregion
}
