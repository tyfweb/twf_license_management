using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;

namespace TechWayFit.Licensing.Management.Infrastructure.Implementations.Repositories.OperationsDashboard;

public class OperationsBaseRepository<TEntity> :
        IOperationsDashboardBaseRepository<TEntity> where TEntity : BaseDbEntity
{
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly LicensingDbContext _context;
    
    public OperationsBaseRepository(LicensingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }
    
    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.CreatedOn = DateTime.UtcNow;
        entity.IsActive = true;
        
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IEnumerable<TEntity>> CreateBulkAsync(IEnumerable<TEntity> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entityList = entities.ToList();
        if (!entityList.Any())
            return entityList;

        var currentTime = DateTime.UtcNow;
        foreach (var entity in entityList)
        {
            entity.CreatedOn = currentTime;
            entity.IsActive = true;
        }

        await _dbSet.AddRangeAsync(entityList);
        await _context.SaveChangesAsync();
        return entityList;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOlderThanAsync(DateTime cutoffDate)
    {
        var entitiesToDelete = await _dbSet
            .Where(e => e.IsActive && e.CreatedOn < cutoffDate)
            .ToListAsync();
            
        if (entitiesToDelete.Any())
        {
            var currentTime = DateTime.UtcNow;
            foreach (var entity in entitiesToDelete)
            {
                entity.IsActive = false;
                entity.UpdatedOn = currentTime;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.Where(e => e.IsActive).AnyAsync(e => EF.Property<Guid>(e, GetIdPropertyName()) == id);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Where(e => e.IsActive).FirstOrDefaultAsync(e => EF.Property<Guid>(e, GetIdPropertyName()) == id);
    }

    public async Task<IEnumerable<TEntity>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Where(e => e.IsActive && e.CreatedOn >= startTime && e.CreatedOn <= endTime)
            .OrderByDescending(e => e.CreatedOn)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _dbSet.Where(e => e.IsActive).CountAsync();
    }

    public async Task<int> GetCountByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        return await _dbSet.CountAsync(e => e.IsActive && e.CreatedOn >= startTime && e.CreatedOn <= endTime);
    }
    
    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.UpdatedOn = DateTime.UtcNow;
        
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Gets the name of the ID property for the specific entity type
    /// Override this in derived classes to specify the correct ID property name
    /// </summary>
    protected virtual string GetIdPropertyName()
    {
        return "Id"; // Default - should be overridden by derived classes
    }
}