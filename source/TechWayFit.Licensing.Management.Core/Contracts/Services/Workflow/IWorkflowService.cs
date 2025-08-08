using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Core.Models.Workflow;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow;

/// <summary>
/// Generic interface for managing entity workflow operations
/// </summary>
/// <typeparam name="T">The model type that implements IWorkflowCapable</typeparam>
public interface IWorkflowService<T> where T : IWorkflowCapable
{
    /// <summary>
    /// Submit an entity for approval
    /// </summary>
    Task<T> SubmitForApprovalAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve an entity
    /// </summary>
    Task<T> ApproveAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reject an entity
    /// </summary>
    Task<T> RejectAsync(Guid entityId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraw an entity from approval
    /// </summary>
    Task<T> WithdrawAsync(Guid entityId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities pending approval
    /// </summary>
    Task<IEnumerable<T>> GetPendingApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities for a specific user
    /// </summary>
    Task<IEnumerable<T>> GetUserEntitiesAsync(string userId, EntityStatus? status = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow history for an entity
    /// </summary>
    Task<IEnumerable<WorkflowHistoryEntry>> GetWorkflowHistoryAsync(Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a user can approve entities
    /// </summary>
    Task<bool> CanUserApproveAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move entity to next status in the workflow
    /// </summary>
    Task<T> MoveToNextStatusAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move entity to previous status in the workflow
    /// </summary>
    Task<T> MoveToPreviousStatusAsync(Guid entityId, string? comments = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specific workflow service for ConsumerAccount
/// </summary>
public interface IConsumerAccountWorkflowService : IWorkflowService<ConsumerAccount>
{
}

/// <summary>
/// Specific workflow service for EnterpriseProduct
/// </summary>
public interface IEnterpriseProductWorkflowService : IWorkflowService<EnterpriseProduct>
{
}

/// <summary>
/// Specific workflow service for ProductLicense
/// </summary>
public interface IProductLicenseWorkflowService : IWorkflowService<ProductLicense>
{
}

/// <summary>
/// Workflow history entry for tracking entity status changes
/// </summary>

