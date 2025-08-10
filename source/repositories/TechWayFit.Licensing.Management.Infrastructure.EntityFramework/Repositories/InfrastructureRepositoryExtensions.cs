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
