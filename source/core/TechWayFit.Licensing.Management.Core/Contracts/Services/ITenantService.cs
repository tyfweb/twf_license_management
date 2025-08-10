using TechWayFit.Licensing.Management.Core.Models.Tenant;

namespace TechWayFit.Licensing.Management.Core.Contracts.Services;

/// <summary>
/// Service interface for tenant management operations
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Get all tenants
    /// </summary>
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new tenant
    /// </summary>
    Task<Tenant> CreateTenantAsync(
        string tenantName,
        string? tenantCode = null,
        string? description = null,
        string? website = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    Task<bool> UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a tenant
    /// </summary>
    Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if tenant exists
    /// </summary>
    Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if tenant code is unique
    /// </summary>
    Task<bool> IsTenantCodeUniqueAsync(string tenantCode, Guid? excludeTenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a tenant
    /// </summary>
    Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a tenant
    /// </summary>
    Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
