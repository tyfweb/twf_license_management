using System.Diagnostics;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;

/// <summary>
/// Enhanced base repository mixin - infrastructure concerns only
/// Focuses on validation, security, and metrics - NOT logging
/// </summary>
public abstract class BaseRepository<TModel, TEntity> :
    AuditRepository<TModel, TEntity>
    where TEntity : BaseEntity, IEntityMapper<TModel, TEntity>, new()
    where TModel : class, new()
{
    public BaseRepository(EfCoreLicensingDbContext context, IUserContext userContext)
        : base(context, userContext)
    {
        
    }
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
