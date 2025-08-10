using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data; 
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Services.Implementations.Audit;

/// <summary>
/// Implementation of the Audit service
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IUnitOfWork unitOfWork,
        ILogger<AuditService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an audit entry
    /// </summary>
    public async Task<string> LogAuditEntryAsync(AuditEntry entry)
    {
        _logger.LogInformation("Logging audit entry: {Action} for {EntityType}", entry.ActionType, entry.EntityType);
        // Ensure the entry is valid
        Ensure.NotNull(entry, nameof(entry));
        Ensure.NotEmpty(entry.ActionType, nameof(entry.ActionType));
        Ensure.NotEmpty(entry.EntityType, nameof(entry.EntityType));

        try
        {
            // Save to repository
            var createdEntity = await _unitOfWork.AuditEntries.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully logged audit entry with ID: {AuditId}", createdEntity.EntityId);
            return createdEntity.EntryId.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit entry: {Action}", entry.ActionType);
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets audit entries for a specific license
    /// </summary>
    public async Task<IEnumerable<AuditEntry>> GetLicenseAuditEntriesAsync(
        Guid licenseId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("LicenseId cannot be empty", nameof(licenseId));

        try
        {
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>
                {
                    { "EntityType", "License" },
                    { "EntityId", licenseId.ToString() }
                },
                FromDate = fromDate,
                ToDate = toDate         
            };
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
 

            return entities.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting license audit entries: {LicenseId}", licenseId);
            throw;
        }
    }

    /// <summary>
    /// Gets audit entries for a specific consumer
    /// </summary>
    public async Task<IEnumerable<AuditEntry>> GetConsumerAuditEntriesAsync(
        Guid consumerId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        if (consumerId == Guid.Empty)
            throw new ArgumentException("ConsumerId cannot be empty", nameof(consumerId));

        try
        {
           
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>
                {
                    { "EntityType", "EntityType" },
                    { "EntityId", consumerId.ToString() }
                },
                FromDate = fromDate,
                ToDate = toDate         
            };
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
           return entities.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer audit entries: {ConsumerId}", consumerId);
            throw;
        }
    }

   

    public async Task<IEnumerable<AuditEntry>> GetProductAuditEntriesAsync(
        string productId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement
        _logger.LogWarning("GetProductAuditEntriesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<AuditEntry>();
    }    public async Task<IEnumerable<AuditEntry>> GetUserAuditEntriesAsync(
        string userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // TODO: Implement
        _logger.LogWarning("GetUserAuditEntriesAsync not implemented");
        await Task.CompletedTask;
        return Enumerable.Empty<AuditEntry>();
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditEntriesByActionAsync(
        string action,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>
                {
                    { "ActionType", action }
                },
                FromDate = fromDate,
                ToDate = toDate         
            };
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
           return entities.Results;            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit entries by action: {Action}", action);
            return Enumerable.Empty<AuditEntry>();
        }
    }

    public async Task<IEnumerable<string>> GetDistinctActionsAsync()
    {
        try
        {
            var entities = await _unitOfWork.AuditEntries.GetRecentEntriesAsync(1000);
            return entities.Select(e => e.ActionType).Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distinct actions");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<string>> GetDistinctEntityTypesAsync()
    {
        try
        {
            var entities = await _unitOfWork.AuditEntries.GetRecentEntriesAsync(1000);
            return entities.Select(e => e.EntityType).Distinct().ToList();
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distinct entity types");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<AuditEntry>> GetSecurityAuditEntriesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>
                {
                    { "ActionType", "LOGIN" },
                    { "ActionType", "LOGOUT" },
                    { "ActionType", "ACCESS_DENIED" },
                    { "ActionType", "PERMISSION_CHANGE" },
                    { "ActionType", "DELETE" },
                    { "ActionType", "ADMIN_ACCESS" }
                },
                FromDate = fromDate,
                ToDate = toDate         
            };
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
            return entities.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security audit entries");
            return Enumerable.Empty<AuditEntry>();
        }
    }

    // Additional missing interface methods - implementing as TODOs
    public async Task<IEnumerable<AuditEntry>> GetAuditEntriesAsync(
        string? entityType = null,
        string? entityId = null,
        string? actionType = null,
        string? userName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Page = pageNumber,
                PageSize = pageSize,
                Filters = new Dictionary<string, object>(),
                FromDate = fromDate,
                ToDate = toDate         
            };
            if (!string.IsNullOrEmpty(entityType))
                searchRequest.Filters["EntityType"] = entityType;
            if (!string.IsNullOrEmpty(entityId))
                searchRequest.Filters["EntityId"] = entityId;
            if (!string.IsNullOrEmpty(actionType))
                searchRequest.Filters["ActionType"] = actionType;
            if (!string.IsNullOrEmpty(userName))
                searchRequest.Filters["UserName"] = userName;
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
            return entities.Results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit entries");
            return Enumerable.Empty<AuditEntry>();
        }
    }

    public async Task<int> GetAuditEntryCountAsync(
        string? entityType = null,
        string? entityId = null,
        string? actionType = null,
        string? userName = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
           SearchRequest<AuditEntry> searchRequest = new()
            {
                Filters = new Dictionary<string, object>(),
                FromDate = fromDate,
                ToDate = toDate         
            };
            if (!string.IsNullOrEmpty(entityType))
                searchRequest.Filters["EntityType"] = entityType;
            if (!string.IsNullOrEmpty(entityId))
                searchRequest.Filters["EntityId"] = entityId;
            if (!string.IsNullOrEmpty(actionType))
                searchRequest.Filters["ActionType"] = actionType;
            if (!string.IsNullOrEmpty(userName))
                searchRequest.Filters["UserName"] = userName;
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
            //TODO: Implement CountAsync method in repository
            return entities.TotalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit entry count");
            return 0;
        }
    }

    public async Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            SearchRequest<AuditEntry> searchRequest = new()
            {
                Filters = new Dictionary<string, object>(),
                FromDate = fromDate,
                ToDate = toDate         
            }; 
            // Use repository method if available
            var entities = await _unitOfWork.AuditEntries.SearchAsync(searchRequest);
             
            return new AuditStatistics
            {
                TotalEntries = entities.TotalCount,
                EntriesByAction = entities.Results.GroupBy(e => e.ActionType).ToDictionary(g => g.Key, g => g.Count()),
                EntriesByEntity = entities.Results.GroupBy(e => e.EntityType).ToDictionary(g => g.Key, g => g.Count()),
                EntriesByUser = entities.Results.GroupBy(e => e.UserName).ToDictionary(g => g.Key, g => g.Count()),
                EntriesByDate = entities.Results.GroupBy(e => e.Timestamp).ToDictionary(g => g.Key, g => g.Count()),
                UniqueUsers = entities.Results.Select(e => e.UserName).Distinct().Count(),
                UniqueEntities = entities.Results.Select(e => $"{e.EntityType}:{e.EntityId}").Distinct().Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit statistics");
            return new AuditStatistics();
        }
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditEntriesAsync(AuditSearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        Ensure.NotNull(searchRequest, nameof(searchRequest)); 
        var entitySearchRequest = new SearchRequest<AuditEntry>
        {
            Page = searchRequest.PageNumber,
            PageSize = searchRequest.PageSize,
            Filters = new Dictionary<string, object>(),
            FromDate = searchRequest.FromDate,
            ToDate = searchRequest.ToDate
        };
        if (searchRequest.EntityType != null)
            entitySearchRequest.Filters.Add(nameof(searchRequest.EntityType), searchRequest.EntityType.Value.ToString());
        if (searchRequest.ActionType != null)
            entitySearchRequest.Filters.Add(nameof(searchRequest.ActionType), searchRequest.ActionType.Value.ToString());
        if (!string.IsNullOrEmpty(searchRequest.EntityId))
            entitySearchRequest.Filters.Add(nameof(searchRequest.EntityId), searchRequest.EntityId);

        var searchResults = await _unitOfWork.AuditEntries.SearchAsync(entitySearchRequest, cancellationToken);

        return searchResults.Results;
    }
 
}
