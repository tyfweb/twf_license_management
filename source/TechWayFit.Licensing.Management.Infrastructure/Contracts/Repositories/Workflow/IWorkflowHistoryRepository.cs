using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;
using TechWayFit.Licensing.Management.Core.Models.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;

/// <summary>
/// Repository interface for workflow history tracking
/// </summary>
public interface IWorkflowHistoryRepository : IBaseRepository<WorkflowHistoryEntry>
{
    /// <summary>
    /// Get workflow history for a specific entity
    /// </summary>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history entries</returns>
    Task<IEnumerable<WorkflowHistoryEntry>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow history for a specific entity type
    /// </summary>
    /// <param name="entityType">Type of the entity</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history entries</returns>
    Task<IEnumerable<WorkflowHistoryEntry>> GetByEntityTypeAsync(string entityType, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a workflow action
    /// </summary>
    /// <param name="historyEntry">Workflow history entry to record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created workflow history entry</returns>
    Task<WorkflowHistoryEntry> RecordActionAsync(WorkflowHistoryEntry historyEntry, CancellationToken cancellationToken = default);
}
