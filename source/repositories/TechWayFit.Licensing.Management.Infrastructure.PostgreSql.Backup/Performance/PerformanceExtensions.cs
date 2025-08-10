using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Performance;

/// <summary>
/// Performance optimization extensions for PostgreSQL infrastructure
/// </summary>
public static class PerformanceExtensions
{
    /// <summary>
    /// Configure PostgreSQL for optimal performance
    /// </summary>
    public static void ConfigurePerformance(this DbContextOptionsBuilder options, string connectionString)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Enable command batching for better performance
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
            
            // Set optimal command timeout
            npgsqlOptions.CommandTimeout(30);
        })
        .UseSnakeCaseNamingConvention()
        // TODO: Configure query splitting - requires proper EF Core version
        // .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        // Configure model caching
        .EnableServiceProviderCaching()
        .EnableSensitiveDataLogging(false); // Never enable in production
    }

    /// <summary>
    /// Add query caching for frequently accessed data
    /// </summary>
    public static IServiceCollection AddQueryCaching(this IServiceCollection services)
    {
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000; // Limit cache size
            options.CompactionPercentage = 0.25; // Remove 25% when limit reached
        });

        services.AddSingleton<IQueryCache, QueryCache>();
        return services;
    }
}

/// <summary>
/// Query caching interface for frequently accessed data
/// </summary>
public interface IQueryCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory query cache implementation
/// </summary>
public class QueryCache : IQueryCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<QueryCache> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _cacheKeys = new();

    public QueryCache(IMemoryCache cache, ILogger<QueryCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
            Priority = CacheItemPriority.Normal,
            Size = 1
        };

        _cache.Set(key, value, options);
        _cacheKeys.TryAdd(key, DateTime.UtcNow);

        _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        _logger.LogDebug("Removed cache entry for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }

        _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Query optimization helper
/// </summary>
public static class QueryOptimizer
{
    /// <summary>
    /// Optimize query with proper includes and filters
    /// </summary>
    public static IQueryable<T> OptimizeQuery<T>(
        this IQueryable<T> query,
        string[]? includes = null,
        int? maxResults = null,
        bool splitQuery = true) where T : class
    {
        // Apply includes if specified
        if (includes?.Length > 0)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        // Use split query for complex includes
        if (splitQuery && includes?.Length > 1)
        {
            query = query.AsSplitQuery();
        }

        // Limit results to prevent memory issues
        if (maxResults.HasValue)
        {
            query = query.Take(maxResults.Value);
        }

        return query;
    }

    /// <summary>
    /// Create cache key for entity queries
    /// </summary>
    public static string CreateCacheKey<T>(string operation, object? parameters = null, Guid? tenantId = null)
    {
        var entityType = typeof(T).Name;
        var paramHash = parameters?.GetHashCode().ToString() ?? "none";
        var tenant = tenantId?.ToString() ?? "global";
        
        return $"{entityType}:{operation}:{paramHash}:{tenant}";
    }
}
