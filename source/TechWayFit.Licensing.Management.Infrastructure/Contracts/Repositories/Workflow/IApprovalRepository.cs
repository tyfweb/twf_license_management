using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;

/// <summary>
/// Repository interface for managing entity approval workflows at the database level
/// </summary>
/// <typeparam name="TEntity">Type of the database entity that inherits from BaseDbEntity</typeparam>
public interface IApprovalRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get all entities with a specific status
    /// </summary>
    /// <param name="status">Entity status to filter by</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities with the specified status</returns>
    Task<IEnumerable<TEntity>> GetByStatusAsync(EntityStatus status, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities pending approval
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities pending approval</returns>
    Task<IEnumerable<TEntity>> GetPendingApprovalAsync(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities created or submitted by a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">Optional filter by status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities for the user</returns>
    Task<IEnumerable<TEntity>> GetUserEntitiesAsync(string userId, EntityStatus? status = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity status
    /// </summary>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="newStatus">New status</param>
    /// <param name="actionBy">User performing the action</param>
    /// <param name="comments">Optional comments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entity</returns>
    Task<TEntity> UpdateStatusAsync(Guid entityId, EntityStatus newStatus, string actionBy, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of entities by status
    /// </summary>
    /// <param name="status">Entity status to count</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of entities with the specified status</returns>
    Task<int> GetCountByStatusAsync(EntityStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of pending approvals for dashboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of entities pending approval</returns>
    Task<int> GetPendingApprovalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entities submitted by a user that are pending review
    /// </summary>
    /// <param name="userId">User ID who submitted the entities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities submitted by the user that are pending</returns>
    Task<IEnumerable<TEntity>> GetUserPendingSubmissionsAsync(string userId, CancellationToken cancellationToken = default);
}
