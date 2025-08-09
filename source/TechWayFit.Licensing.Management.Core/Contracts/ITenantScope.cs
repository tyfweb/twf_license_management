namespace TechWayFit.Licensing.Management.Core.Contracts;

/// <summary>
/// Interface for managing tenant scope during operations
/// </summary>
public interface ITenantScope
{
    /// <summary>
    /// Gets the current effective tenant ID
    /// </summary>
    Guid? CurrentTenantId { get; }

    /// <summary>
    /// Gets the current effective username
    /// </summary>
    string? CurrentUsername { get; }

    /// <summary>
    /// Creates a system tenant scope for system operations
    /// </summary>
    /// <returns>A disposable scope that restores the previous tenant context</returns>
    IDisposable CreateSystemScope();

    /// <summary>
    /// Creates a specific tenant scope
    /// </summary>
    /// <param name="tenantId">The tenant ID to scope to</param>
    /// <returns>A disposable scope that restores the previous tenant context</returns>
    IDisposable CreateTenantScope(Guid tenantId);

    /// <summary>
    /// Checks if currently in system scope
    /// </summary>
    bool IsSystemScope { get; }
}
