using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Helpers;

namespace TechWayFit.Licensing.Management.Core.Extensions;

/// <summary>
/// Extension methods for tenant scope operations
/// </summary>
public static class TenantScopeExtensions
{
    /// <summary>
    /// Executes an async operation within a system tenant scope
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="tenantScope">The tenant scope service</param>
    /// <param name="operation">The operation to execute</param>
    /// <returns>The result of the operation</returns>
    public static async Task<T> ExecuteInSystemScopeAsync<T>(this ITenantScope tenantScope, Func<Task<T>> operation)
    {
        using var scope = tenantScope.CreateSystemScope();
        return await operation();
    }

    /// <summary>
    /// Executes an operation within a system tenant scope
    /// </summary>
    /// <param name="tenantScope">The tenant scope service</param>
    /// <param name="operation">The operation to execute</param>
    public static async Task ExecuteInSystemScopeAsync(this ITenantScope tenantScope, Func<Task> operation)
    {
        using var scope = tenantScope.CreateSystemScope();
        await operation();
    }

    /// <summary>
    /// Executes an async operation within a specific tenant scope
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="tenantScope">The tenant scope service</param>
    /// <param name="tenantId">The tenant ID to scope to</param>
    /// <param name="operation">The operation to execute</param>
    /// <returns>The result of the operation</returns>
    public static async Task<T> ExecuteInTenantScopeAsync<T>(this ITenantScope tenantScope, Guid tenantId, Func<Task<T>> operation)
    {
        using var scope = tenantScope.CreateTenantScope(tenantId);
        return await operation();
    }

    /// <summary>
    /// Executes an operation within a specific tenant scope
    /// </summary>
    /// <param name="tenantScope">The tenant scope service</param>
    /// <param name="tenantId">The tenant ID to scope to</param>
    /// <param name="operation">The operation to execute</param>
    public static async Task ExecuteInTenantScopeAsync(this ITenantScope tenantScope, Guid tenantId, Func<Task> operation)
    {
        using var scope = tenantScope.CreateTenantScope(tenantId);
        await operation();
    }

    /// <summary>
    /// Checks if a given tenant ID is the system tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID to check</param>
    /// <returns>True if it's the system tenant, false otherwise</returns>
    public static bool IsSystemTenant(this Guid tenantId)
    {
        return tenantId == IdConstants.SystemTenantId;
    }

    /// <summary>
    /// Gets the system tenant ID
    /// </summary>
    /// <returns>The system tenant ID</returns>
    public static Guid GetSystemTenantId()
    {
        return IdConstants.SystemTenantId;
    }
}
