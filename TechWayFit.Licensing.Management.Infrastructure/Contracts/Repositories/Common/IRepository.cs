using System.Linq.Expressions;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;

/// <summary>
/// Generic repository interface that provides basic CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity or null if not found</returns>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities with optional filter, ordering, and pagination
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="orderBy">Ordering function</param>
    /// <param name="includeProperties">Properties to include</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entities</returns>
    Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of entities with optional filter
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of entities</returns>
    Task<int> GetCountAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get first entity matching the filter
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="includeProperties">Properties to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>First matching entity or null</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> filter,
        string includeProperties = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any entity matches the filter
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches</returns>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entity</returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entity</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    /// <param name="entities">Entities to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if changes were saved successfully, false otherwise</returns>
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
