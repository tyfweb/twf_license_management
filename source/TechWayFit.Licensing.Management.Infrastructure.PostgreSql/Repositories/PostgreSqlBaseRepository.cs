using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;

/// <summary>
/// PostgreSQL implementation of base repository
/// </summary>
public partial class PostgreSqlBaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : BaseAuditEntity
{
    protected readonly PostgreSqlPostgreSqlLicensingDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public PostgreSqlBaseRepository(PostgreSqlPostgreSqlLicensingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }
    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        var entity = await _dbSet.FindAsync(id, cancellationToken);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="includes"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync();
    }
    /// <summary>
    /// Retrieves entities with optional includes.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> IncludesAsync(IQueryable<TEntity> query, params Expression<Func<TEntity, object>>[] includes)
    {
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return await query.ToListAsync();
    }
    /// <summary>
    /// Retrieves an entity by its ID with optional includes.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        return await _dbSet.FindAsync(id, cancellationToken);
    }
    /// <summary>
    /// Searches for entities based on the provided request.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<SearchResponse<TEntity>> SearchAsync(SearchRequest<TEntity> request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        int page = request.Page > 0 ? request.Page : 1;
        int pageSize = request.PageSize > 0 ? request.PageSize : 20;
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = SearchQuery(query, request.Query.Trim());
        }
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filter in request.Filters)
            {
                if (filter is Expression<Func<TEntity, bool>> expression)
                    query = query.Where(expression);
                else
                    throw new ArgumentException("All filters must be expressions of type Expression<Func<TEntity, bool>>");
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = SearchIncludesQuery(query);
        var items = await query.Skip((request.Page - 1) * request.PageSize)
                                .Take(request.PageSize)
                                .ToListAsync(cancellationToken);

        return new SearchResponse<TEntity>(totalCount, page, pageSize, items);
    }
    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        var entity = await _dbSet.FindAsync(id, cancellationToken);
        return entity?.IsActive ?? false;
    }

    public async Task<bool> MarkAsInactiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        var entity = await _dbSet.FindAsync(id, cancellationToken);
        if (entity == null)
            return false;

        entity.IsActive = false;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(DateTime? createdAfter = null, DateTime? createdBefore = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        if (createdAfter.HasValue)
        {
            query = query.Where(e => e.CreatedOn >= createdAfter.Value);
        }
        if (createdBefore.HasValue)
        {
            query = query.Where(e => e.CreatedOn <= createdBefore.Value);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(SearchRequest<TEntity> request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = SearchQuery(query, request.Query.Trim());
        }
        if (request.Filters != null)
        {
            if (request.Filters is Expression<Func<TEntity, bool>> filter)
                query = query.Where(filter);
            else
                throw new ArgumentException("Filters must be an expression of type Expression<Func<TEntity, bool>>");
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        return entity;
    }



    protected IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, params Expression<Func<TEntity, object>>[] includes)
    {
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
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

    public async Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(id, cancellationToken);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.DeletedBy = deletedBy;
        entity.DeletedOn = DateTime.UtcNow;

        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<bool> ExistsAsync(Guid id,bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));
        if (includeDeleted)
            return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted && e.IsActive, cancellationToken);
    }
}
