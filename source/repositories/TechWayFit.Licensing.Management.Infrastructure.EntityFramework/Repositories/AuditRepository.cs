using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Contracts;
using System.Runtime.InteropServices.Marshalling;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

/// <summary>
/// PostgreSQL implementation of base repository
/// </summary>
public partial class AuditRepository<TModel, TEntity> : IDataRepository<TModel> 
    where TEntity : AuditEntity, IEntityMapper<TModel, TEntity>, new()
    where TModel : class, new()

{
    protected readonly EfCoreLicensingDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IUserContext _userContext;


    public AuditRepository(EfCoreLicensingDbContext context, IUserContext userContext)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        if (context == null) throw new ArgumentNullException(nameof(context));

        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    public virtual Task<TModel> AddAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = new TEntity();
        entity.Map(model);
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        } 
        entity.IsDeleted = false;
        entity.IsActive = true;
        entity.CreatedOn = DateTime.UtcNow;
        entity.CreatedBy = _userContext.UserName ?? "Anonymous";
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";
        entity.UpdatedOn = DateTime.UtcNow;
        _dbSet.Add(entity);
        
        return Task.FromResult(entity.Map());
    }

    public virtual Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = _dbSet.Find(id);
        if (entity == null) return Task.FromResult(false);
        
        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.UtcNow;
        entity.DeletedBy = _userContext.UserName ?? "Anonymous";
        entity.IsActive = false;
        entity.UpdatedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";
        
        return Task.FromResult(true);
    }
    /// <summary>
    /// Checks if an entity exists by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeDeleted"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async virtual  Task<bool> ExistsAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        if (!includeDeleted)
        {
            query = query.Where(e => !e.IsDeleted);
        }
        return await query.AnyAsync(e => e.Id == id, cancellationToken);
    }
    /// <summary>
    /// Finds a single entity based on the provided search request.
    /// If multiple entities match, the first one is returned.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async virtual  Task<TModel?> FindOneAsync(SearchRequest<TModel> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var query = _dbSet.AsQueryable();

        if (request.Query != null)
        {
            query = SearchQuery(query, request.Query);
        }
        query = SearchIncludesQuery(query);
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filter in request.Filters)
            {
                query = query.Where(e => EF.Property<string>(e, filter.Key) == (string)filter.Value);
            }
        }

        return (await query.FirstOrDefaultAsync(cancellationToken))?.Map();
    }
    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async virtual  Task<IEnumerable<TModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        query = ApplyIncludes(query);
        return (await query.ToListAsync(cancellationToken))
            .Select(e => e.Map());
    }
    /// <summary>
    /// Retrieves all entities with optional date filters.
    /// </summary>
    /// <param name="createdAfter"></param>
    /// <param name="createdBefore"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TModel>> GetAllAsync(DateTime? createdAfter = null, DateTime? createdBefore = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (createdAfter.HasValue)
        {
            query = query.Where(e => e.CreatedOn >= createdAfter.Value);
        }

        if (createdBefore.HasValue)
        {
            query = query.Where(e => e.CreatedOn <= createdBefore.Value);
        }

        return (await _dbSet.AsNoTracking()
            .ToListAsync(cancellationToken))
            .Select(e => e.Map());
    }
    /// <summary>
    /// Retrieves all entities with optional includes.
    /// </summary>
    /// <returns></returns>
    public async virtual  Task<IEnumerable<TModel>> GetAllWithIncludesAsync()
    {
        var query = _dbSet.AsNoTracking().AsQueryable();
        query = ApplyIncludes(query);
        return (await query.ToListAsync())
            .Select(e => e.Map());
    }
    /// <summary>
    /// Retrieves an entity from the repository by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async virtual  Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        query = ApplyIncludes(query);
        var entity = await query.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        return entity?.Map();
    }
    /// <summary>
    /// Checks if an entity is active by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async virtual Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .AnyAsync(e => e.Id == id && e.IsActive && !e.IsDeleted, cancellationToken);
    }
    /// <summary>
    /// Marks an entity as inactive by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual Task<bool> MarkAsInactiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = _dbSet.Find(id);
        if (entity == null) return Task.FromResult(false);

        entity.IsActive = false;
        entity.UpdatedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";
        
        return Task.FromResult(true);
    }
    /// <summary>
    /// Searches for entities based on the provided search request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async virtual  Task<SearchResponse<TModel>> SearchAsync(SearchRequest<TModel> request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (request.Query != null)
        {
            query = SearchQuery(query, request.Query);
        }
        query = SearchIncludesQuery(query);
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filter in request.Filters)
            {
                query = query.Where(e => EF.Property<string>(e, filter.Key) == (string)filter.Value);
            }
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var data_query = query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var result = (await data_query.ToListAsync(cancellationToken)).Select(e => e.Map());
        return new SearchResponse<TModel>(totalCount, request.Page, request.PageSize, result);
    }
    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public virtual Task<TModel> UpdateAsync(Guid id, TModel model, CancellationToken cancellationToken = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var entity = _dbSet.Find(id);
        if (entity == null) throw new KeyNotFoundException($"Entity with ID {id} not found.");

        entity.Map(model);
        entity.UpdatedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";

        _context.Entry(entity).State = EntityState.Modified;
        return Task.FromResult(entity.Map());
    }

    public Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = _dbSet.Find(id);
        if (entity == null) return Task.FromResult(false);

        entity.IsActive = true;
        entity.UpdatedOn = DateTime.UtcNow;
        entity.UpdatedBy = _userContext.UserName ?? "Anonymous";

        return Task.FromResult(true);
    }

    protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> SearchQuery(IQueryable<TEntity> query, string searchQuery)
    {
        return query;
    }
    protected virtual IQueryable<TEntity> SearchIncludesQuery(IQueryable<TEntity> query)
    {
        return query;
    }
 
}
