using System.Linq.Expressions;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

public interface IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository by its ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an entity from the repository by its ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves all entities with optional date filters.
    /// </summary>
    /// <param name="createdAfter"></param>
    /// <param name="createdBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetAllAsync(DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity is active by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsActiveAsync(string id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Marks an entity as inactive by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> MarkAsInactiveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for entities based on the provided search request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResponse<TEntity>> SearchAsync(
        SearchRequest<TEntity> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity based on the provided search request.
    /// If multiple entities match, the first one is returned.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity?> FindOneAsync(
        SearchRequest<TEntity> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities with optional includes.
    /// </summary>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(
        params Expression<Func<TEntity, object>>[] includes);
}
