using System.Linq.Expressions;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;

public interface IDataRepository<TModel> where TModel : class
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task<TModel> AddAsync(TModel entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the repository by its ID.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeDeleted"></param>
    /// <param name="cancellationToken"></param>
    Task<bool> ExistsAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity based on the provided search request.
    /// If multiple entities match, the first one is returned.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TModel?> FindOneAsync(
        SearchRequest<TModel> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities with optional date filters.
    /// </summary>
    /// <param name="createdAfter"></param>
    /// <param name="createdBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> GetAllAsync(DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities with optional includes.
    /// </summary>
    /// <param name="includes"></param>
    /// <returns></returns>
    Task<IEnumerable<TModel>> GetAllWithIncludesAsync();

    /// <summary>
    /// Retrieves an entity from the repository by its ID.
    /// </summary>
    Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity is active by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an entity as inactive by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> MarkAsInactiveAsync(Guid id, CancellationToken cancellationToken = default);

      /// <summary>
    /// Activates an entity by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for entities based on the provided search request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResponse<TModel>> SearchAsync(
        SearchRequest<TModel> request,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    Task<TModel> UpdateAsync(Guid id,TModel entity, CancellationToken cancellationToken = default);
}
