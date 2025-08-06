using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Workflow;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;

/// <summary>
/// Repository interface for workflow history tracking
/// </summary>
public interface IWorkflowHistoryRepository : IBaseRepository<WorkflowHistoryEntity>
{
    /// <summary>
    /// Get workflow history for a specific entity
    /// </summary>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history entries</returns>
    Task<IEnumerable<WorkflowHistoryEntity>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow history for a specific entity type
    /// </summary>
    /// <param name="entityType">Type of the entity</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history entries</returns>
    Task<IEnumerable<WorkflowHistoryEntity>> GetByEntityTypeAsync(string entityType, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a workflow action
    /// </summary>
    /// <param name="historyEntry">Workflow history entry to record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workflow history entry</returns>
    Task<WorkflowHistoryEntity> RecordActionAsync(WorkflowHistoryEntity historyEntry, CancellationToken cancellationToken = default);
}
