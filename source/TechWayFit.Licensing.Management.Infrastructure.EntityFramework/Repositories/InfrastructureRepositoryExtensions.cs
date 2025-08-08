using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

/// <summary>
/// Infrastructure-specific repository extensions - NO LOGGING (handled by EF middleware)
/// Focus: Business validation, security, and lightweight metrics only
/// </summary>
public static class RepositoryInfrastructureExtensions
{
    /// <summary>
    /// Validate business model before database operations
    /// </summary>
    public static void ValidateBusinessRules<TModel>(TModel model) where TModel : class
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        
        if (!Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
            throw new ArgumentException($"Model validation failed: {errors}", nameof(model));
        }
    }

    /// <summary>
    /// Validate tenant access for multi-tenant security
    /// </summary>
    public static void ValidateTenantAccess<TEntity>(TEntity entity, IUserContext userContext) 
        where TEntity : BaseEntity
    {
        var userTenantId= userContext?.TenantId; 
        if (entity.TenantId != Guid.Empty && 
            entity.TenantId != userTenantId)
        {
            throw new UnauthorizedAccessException(
                $"Access denied to entity {entity.Id} from tenant {entity.TenantId}. Current user tenant: {userTenantId}");
        }
    }

    /// <summary>
    /// Collect basic performance metrics (NO detailed logging - that's handled by EF middleware)
    /// </summary>
    public static void CollectInfrastructureMetrics(
        string operationName, 
        string entityType, 
        TimeSpan duration, 
        bool isSuccess, 
        string? tenantId = null)
    {
        // Infrastructure-level metrics only (NOT detailed logging)
        // This would integrate with your monitoring system
        // Examples: Prometheus counters, Application Insights custom metrics
        
        // TODO: Replace with actual monitoring integration
        // Example patterns:
        // - Increment operation counters
        // - Record operation duration histograms  
        // - Track error rates by tenant/entity type
        
        var metricsData = new
        {
            Operation = operationName,
            EntityType = entityType,
            Duration = duration.TotalMilliseconds,
            Success = isSuccess,
            TenantId = tenantId,
            Timestamp = DateTime.UtcNow
        };
        
        // Placeholder - integrate with actual metrics collection
        _ = metricsData;
    }
}

/// <summary>
/// Enhanced base repository mixin - infrastructure concerns only
/// Focuses on validation, security, and metrics - NOT logging
/// </summary>
public abstract partial class BaseRepository<TModel, TEntity> 
    where TEntity : BaseEntity, IEntityMapper<TModel, TEntity>, new()
    where TModel : class, new()
{
    /// <summary>
    /// Execute repository operation with infrastructure-level concerns
    /// (Validation, Security, Metrics - NOT logging)
    /// </summary>
    protected async Task<T> ExecuteInfrastructureOperationAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        TModel? modelToValidate = null,
        TEntity? entityToValidate = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Pre-operation infrastructure checks
            if (modelToValidate != null)
            {
                RepositoryInfrastructureExtensions.ValidateBusinessRules(modelToValidate);
            }
            
            if (entityToValidate != null)
            {
                RepositoryInfrastructureExtensions.ValidateTenantAccess(entityToValidate, _userContext);
            }

            // Execute operation (EF middleware handles all SQL logging)
            var result = await operation();
            
            stopwatch.Stop();
            
            // Collect lightweight infrastructure metrics
            RepositoryInfrastructureExtensions.CollectInfrastructureMetrics(
                operationName, 
                typeof(TEntity).Name, 
                stopwatch.Elapsed, 
                true, 
                _userContext.TenantId.ToString());
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Collect error metrics
            RepositoryInfrastructureExtensions.CollectInfrastructureMetrics(
                operationName, 
                typeof(TEntity).Name, 
                stopwatch.Elapsed, 
                false, 
                _userContext.TenantId.ToString());
            
            // Re-throw with repository context (EF middleware will log the actual exception)
            throw new InvalidOperationException($"Repository operation '{operationName}' failed for {typeof(TEntity).Name}", ex);
        }
    }

    /// <summary>
    /// Execute operation without return value
    /// </summary>
    protected async Task ExecuteInfrastructureOperationAsync(
        Func<Task> operation,
        string operationName,
        TModel? modelToValidate = null,
        TEntity? entityToValidate = null)
    {
        await ExecuteInfrastructureOperationAsync(async () =>
        {
            await operation();
            return true;
        }, operationName, modelToValidate, entityToValidate);
    }
}
