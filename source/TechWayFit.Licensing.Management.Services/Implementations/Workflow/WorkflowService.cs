using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;

namespace TechWayFit.Licensing.Management.Services.Implementations.Workflow;

/// <summary>
/// Generic workflow service implementation for managing entity approval workflows
/// </summary>
/// <typeparam name="TModel">Core model type that implements IWorkflowCapable</typeparam>
public class WorkflowService<TModel> : IWorkflowService<TModel> 
    where TModel : class, IWorkflowCapable
{
    private readonly IApprovalRepository<TModel> _repository;
    private readonly IWorkflowHistoryRepository _historyRepository;
    private readonly IUserContext _userContext;
    private readonly ILogger<WorkflowService<TModel>> _logger;

    public WorkflowService(
        IApprovalRepository<TModel> repository,
        IWorkflowHistoryRepository historyRepository,
        IUserContext userContext,
        ILogger<WorkflowService<TModel>> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TModel> SubmitForApprovalAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting entity {EntityId} for approval", entityId);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.PendingApproval, 
                "Submitted for approval", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.Draft, EntityStatus.PendingApproval, 
                _userContext.UserName ?? "Unknown", "Submitted for approval", cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting entity {EntityId} for approval", entityId);
            throw;
        }
    }

    public async Task<TModel> ApproveAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Approving entity {EntityId} by {UserId}", entityId, _userContext.UserName);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Approved, 
                comments ?? "Approved", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Approved, 
                _userContext.UserName ?? "Unknown", comments ?? "Approved", cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<TModel> RejectAsync(Guid entityId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Rejecting entity {EntityId} by {UserId}", entityId, _userContext.UserName);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Rejected, 
                reason, cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Rejected, 
                _userContext.UserName ?? "Unknown", reason, cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<TModel> WithdrawAsync(Guid entityId, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Withdrawing entity {EntityId} by {UserId}", entityId, _userContext.UserName);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Withdrawn, 
                reason ?? "Withdrawn by submitter", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Withdrawn, 
                _userContext.UserName ?? "Unknown", reason ?? "Withdrawn by submitter", cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<IEnumerable<TModel>> GetPendingApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _repository.GetPendingApprovalAsync(pageNumber, pageSize, cancellationToken);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approval entities");
            throw;
        }
    }

    public async Task<IEnumerable<TModel>> GetUserEntitiesAsync(string userId, EntityStatus? status = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _repository.GetUserEntitiesAsync(userId, status, pageNumber, pageSize, cancellationToken);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<WorkflowHistoryEntry>> GetWorkflowHistoryAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var historyEntities = await _historyRepository.GetByEntityIdAsync(entityId, cancellationToken);
            return historyEntities.OrderByDescending(h => h.ActionDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow history for entity {EntityId}", entityId);
            throw;
        }
    }

    public Task<bool> CanUserApproveAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Simple implementation - in production, this would check user roles/permissions
        // For now, assume all non-empty users can approve
        return Task.FromResult(!string.IsNullOrWhiteSpace(userId) && userId != "Anonymous");
    }

    public async Task<TModel> MoveToNextStatusAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current entity to determine next status
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {entityId} not found");

            var nextStatus = GetNextStatus(entity.Workflow.Status);

            var updatedEntity = await _repository.UpdateStatusAsync(entityId, nextStatus, 
                comments ?? $"Moved to {nextStatus}", cancellationToken);

            await RecordWorkflowActionAsync(entityId, entity.Workflow.Status, nextStatus, 
                _userContext.UserName ?? "Unknown", comments ?? $"Moved to {nextStatus}", cancellationToken);

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving entity {EntityId} to next status", entityId);
            throw;
        }
    }

    public async Task<TModel> MoveToPreviousStatusAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current entity to determine previous status
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {entityId} not found");

            var previousStatus = GetPreviousStatus(entity.Workflow.Status);

            var updatedEntity = await _repository.UpdateStatusAsync(entityId, previousStatus, 
                comments ?? $"Moved back to {previousStatus}", cancellationToken);

            await RecordWorkflowActionAsync(entityId, entity.Workflow.Status, previousStatus, 
                _userContext.UserName ?? "Unknown", comments ?? $"Moved back to {previousStatus}", cancellationToken);

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving entity {EntityId} to previous status", entityId);
            throw;
        }
    }

    private async Task RecordWorkflowActionAsync(Guid entityId, EntityStatus fromStatus, EntityStatus toStatus, 
        string actionBy, string? comments, CancellationToken cancellationToken)
    {
        try
        {
            var historyEntry = new WorkflowHistoryEntry
            {
                EntityId = entityId,
                EntityType = typeof(TModel).Name,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionBy = actionBy,
                ActionDate = DateTime.UtcNow,
                Comments = comments
            };

            await _historyRepository.AddAsync(historyEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record workflow action for entity {EntityId}", entityId);
            // Don't throw - workflow action recording is secondary to the main operation
        }
    }

    private static EntityStatus GetNextStatus(EntityStatus currentStatus)
    {
        return currentStatus switch
        {
            EntityStatus.Draft => EntityStatus.PendingApproval,
            EntityStatus.PendingApproval => EntityStatus.Approved,
            EntityStatus.Rejected => EntityStatus.PendingApproval,
            EntityStatus.Withdrawn => EntityStatus.Draft,
            EntityStatus.Approved => EntityStatus.Archived,
            EntityStatus.Archived => EntityStatus.Archived, // No next status
            _ => throw new InvalidOperationException($"Cannot determine next status for {currentStatus}")
        };
    }

    private static EntityStatus GetPreviousStatus(EntityStatus currentStatus)
    {
        return currentStatus switch
        {
            EntityStatus.PendingApproval => EntityStatus.Draft,
            EntityStatus.Approved => EntityStatus.PendingApproval,
            EntityStatus.Rejected => EntityStatus.Draft,
            EntityStatus.Withdrawn => EntityStatus.Draft,
            EntityStatus.Archived => EntityStatus.Approved,
            EntityStatus.Draft => EntityStatus.Draft, // No previous status
            _ => throw new InvalidOperationException($"Cannot determine previous status for {currentStatus}")
        };
    }
}
