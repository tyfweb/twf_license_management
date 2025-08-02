namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.OperationsDashboard;

/// <summary>
/// Base repository interface for common Operations Dashboard repository operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IOperationsDashboardBaseRepository<TEntity> where TEntity : class
{
    // Basic CRUD operations - common across all repositories
    Task<TEntity> CreateAsync(TEntity entity);
    Task<IEnumerable<TEntity>> CreateBulkAsync(IEnumerable<TEntity> entities);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Time-based queries - common across all repositories
    Task<IEnumerable<TEntity>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);

    // Maintenance operations - common across all repositories
    Task DeleteOlderThanAsync(DateTime cutoffDate);
    Task<int> GetCountAsync();
    Task<int> GetCountByTimeRangeAsync(DateTime startTime, DateTime endTime);
}
