using TechWayFit.Licensing.Management.Infrastructure.Abstractions.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.Abstractions.Contracts.Repositories;

/// <summary>
/// Repository interface for License entities
/// </summary>
public interface ILicenseRepository : IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Get license by its unique license key
    /// </summary>
    Task<TEntity?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all licenses for a specific product
    /// </summary>
    Task<IEnumerable<TEntity>> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all licenses for a specific consumer
    /// </summary>
    Task<IEnumerable<TEntity>> GetByConsumerIdAsync(string consumerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get licenses by status
    /// </summary>
    Task<IEnumerable<TEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get licenses expiring within a specific timeframe
    /// </summary>
    Task<IEnumerable<TEntity>> GetExpiringLicensesAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active licenses for a consumer and product
    /// </summary>
    Task<IEnumerable<TEntity>> GetActiveLicensesAsync(string consumerId, string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a license key is unique
    /// </summary>
    Task<bool> IsLicenseKeyUniqueAsync(string licenseKey, string? excludeId = null, CancellationToken cancellationToken = default);
}
