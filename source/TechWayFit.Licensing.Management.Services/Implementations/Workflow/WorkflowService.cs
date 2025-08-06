using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;

namespace TechWayFit.Licensing.Management.Services.Implementations.Workflow;

/// <summary>
/// Generic workflow service implementation for managing entity approval workflows
/// </summary>
/// <typeparam name="TModel">Core model type that implements IWorkflowCapable</typeparam>
/// <typeparam name="TEntity">Database entity type</typeparam>
public class WorkflowService<TModel, TEntity> : IWorkflowService<TModel> 
    where TModel : IWorkflowCapable 
    where TEntity : class
{
    private readonly IApprovalRepository<TEntity> _repository;
    private readonly IWorkflowHistoryRepository _historyRepository;
    private readonly ILogger<WorkflowService<TModel, TEntity>> _logger;
    private readonly Func<TEntity, TModel> _toModel;
    private readonly Func<TModel, TEntity> _toEntity;

    public WorkflowService(
        IApprovalRepository<TEntity> repository,
        IWorkflowHistoryRepository historyRepository,
        ILogger<WorkflowService<TModel, TEntity>> logger,
        Func<TEntity, TModel> toModel,
        Func<TModel, TEntity> toEntity)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _toModel = toModel ?? throw new ArgumentNullException(nameof(toModel));
        _toEntity = toEntity ?? throw new ArgumentNullException(nameof(toEntity));
    }

    public async Task<TModel> SubmitForApprovalAsync(Guid entityId, string submittedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting entity {EntityId} for approval by {UserId}", entityId, submittedBy);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.PendingApproval, submittedBy, 
                "Submitted for approval", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.Draft, EntityStatus.PendingApproval, 
                submittedBy, "Submitted for approval", cancellationToken);

            return _toModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting entity {EntityId} for approval", entityId);
            throw;
        }
    }

    public async Task<TModel> ApproveAsync(Guid entityId, string approvedBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Approving entity {EntityId} by {UserId}", entityId, approvedBy);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Approved, approvedBy, 
                comments ?? "Approved", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Approved, 
                approvedBy, comments ?? "Approved", cancellationToken);

            return _toModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<TModel> RejectAsync(Guid entityId, string rejectedBy, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Rejecting entity {EntityId} by {UserId}", entityId, rejectedBy);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Rejected, rejectedBy, 
                reason, cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Rejected, 
                rejectedBy, reason, cancellationToken);

            return _toModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting entity {EntityId}", entityId);
            throw;
        }
    }

    public async Task<TModel> WithdrawAsync(Guid entityId, string withdrawnBy, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Withdrawing entity {EntityId} by {UserId}", entityId, withdrawnBy);

            var entity = await _repository.UpdateStatusAsync(entityId, EntityStatus.Withdrawn, withdrawnBy, 
                reason ?? "Withdrawn by submitter", cancellationToken);

            await RecordWorkflowActionAsync(entityId, EntityStatus.PendingApproval, EntityStatus.Withdrawn, 
                withdrawnBy, reason ?? "Withdrawn by submitter", cancellationToken);

            return _toModel(entity);
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
            return entities.Select(_toModel);
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
            return entities.Select(_toModel);
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
            return historyEntities.Select(h => h.ToModel()).OrderByDescending(h => h.ActionDate);
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

    public async Task<TModel> MoveToNextStatusAsync(Guid entityId, string actionBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current entity to determine next status
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {entityId} not found");

            var currentModel = _toModel(entity);
            var nextStatus = GetNextStatus(currentModel.Workflow.Status);

            var updatedEntity = await _repository.UpdateStatusAsync(entityId, nextStatus, actionBy, 
                comments ?? $"Moved to {nextStatus}", cancellationToken);

            await RecordWorkflowActionAsync(entityId, currentModel.Workflow.Status, nextStatus, 
                actionBy, comments ?? $"Moved to {nextStatus}", cancellationToken);

            return _toModel(updatedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving entity {EntityId} to next status", entityId);
            throw;
        }
    }

    public async Task<TModel> MoveToPreviousStatusAsync(Guid entityId, string actionBy, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current entity to determine previous status
            var entity = await _repository.GetByIdAsync(entityId, cancellationToken);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {entityId} not found");

            var currentModel = _toModel(entity);
            var previousStatus = GetPreviousStatus(currentModel.Workflow.Status);

            var updatedEntity = await _repository.UpdateStatusAsync(entityId, previousStatus, actionBy, 
                comments ?? $"Moved back to {previousStatus}", cancellationToken);

            await RecordWorkflowActionAsync(entityId, currentModel.Workflow.Status, previousStatus, 
                actionBy, comments ?? $"Moved back to {previousStatus}", cancellationToken);

            return _toModel(updatedEntity);
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

            var historyEntity = Infrastructure.Models.Entities.Workflow.WorkflowHistoryEntity.FromModel(historyEntry);
            await _historyRepository.RecordActionAsync(historyEntity, cancellationToken);
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
