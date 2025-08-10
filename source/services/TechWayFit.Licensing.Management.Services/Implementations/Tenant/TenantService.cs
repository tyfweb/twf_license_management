using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Contracts.Services;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Services.Implementations.Tenant;

/// <summary>
/// Service implementation for tenant management operations
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IUnitOfWork unitOfWork,
        ILogger<TenantService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Core.Models.Tenant.Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all tenants");
            return await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            throw;
        }
    }

    public async Task<Core.Models.Tenant.Tenant?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving tenant with ID: {TenantId}", tenantId);
            return await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with ID: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<Core.Models.Tenant.Tenant> CreateTenantAsync(
        string tenantName,
        string? tenantCode = null,
        string? description = null,
        string? website = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantName))
                throw new ArgumentException("Tenant name is required", nameof(tenantName));

            // Generate tenant code if not provided
            if (string.IsNullOrWhiteSpace(tenantCode))
            {
                tenantCode = GenerateTenantCode(tenantName);
            }

            // Check if tenant code is unique
            if (!await IsTenantCodeUniqueAsync(tenantCode, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"Tenant code '{tenantCode}' already exists");
            }

            var tenant = new Core.Models.Tenant.Tenant
            {
                TenantId = Guid.NewGuid(),
                TenantName = tenantName.Trim(),
                TenantCode = tenantCode?.Trim().ToUpperInvariant(),
                Description = description?.Trim(),
                Website = website?.Trim()
            };

            _logger.LogInformation("Creating new tenant: {TenantName} with code: {TenantCode}", tenantName, tenantCode);
            
            var createdTenant = await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created tenant: {TenantId}", createdTenant.TenantId);
            return createdTenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {TenantName}", tenantName);
            throw;
        }
    }

    public async Task<bool> UpdateTenantAsync(Core.Models.Tenant.Tenant tenant, CancellationToken cancellationToken = default)
    {
        try
        {
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));

            if (string.IsNullOrWhiteSpace(tenant.TenantName))
                throw new ArgumentException("Tenant name is required", nameof(tenant.TenantName));

            // Check if tenant code is unique (excluding current tenant)
            if (!string.IsNullOrWhiteSpace(tenant.TenantCode) && 
                !await IsTenantCodeUniqueAsync(tenant.TenantCode, tenant.TenantId, cancellationToken))
            {
                throw new InvalidOperationException($"Tenant code '{tenant.TenantCode}' already exists");
            }

            _logger.LogInformation("Updating tenant: {TenantId}", tenant.TenantId);
            
            var result = await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully updated tenant: {TenantId}", tenant.TenantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant: {TenantId}", tenant?.TenantId);
            throw;
        }
    }

    public async Task<bool> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting tenant: {TenantId}", tenantId);
            
            var result = await _unitOfWork.Tenants.DeleteAsync(tenantId, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully deleted tenant: {TenantId}", tenantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> TenantExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.Tenants.ExistsAsync(tenantId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tenant exists: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> IsTenantCodeUniqueAsync(string tenantCode, Guid? excludeTenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantCode))
                return true; // Null or empty codes are considered unique

            var allTenants = await _unitOfWork.Tenants.GetAllAsync(cancellationToken);
            
            return !allTenants.Any(t => 
                string.Equals(t.TenantCode, tenantCode, StringComparison.OrdinalIgnoreCase) &&
                (excludeTenantId == null || t.TenantId != excludeTenantId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking tenant code uniqueness: {TenantCode}", tenantCode);
            throw;
        }
    }

    public async Task<bool> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating tenant: {TenantId}", tenantId);
            
            var result = await _unitOfWork.Tenants.ActivateAsync(tenantId, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully activated tenant: {TenantId}", tenantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> DeactivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating tenant: {TenantId}", tenantId);
            
            var result = await _unitOfWork.Tenants.DeactivateAsync(tenantId, cancellationToken);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully deactivated tenant: {TenantId}", tenantId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tenant: {TenantId}", tenantId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Generate a tenant code from tenant name
    /// </summary>
    private static string GenerateTenantCode(string tenantName)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
            return string.Empty;

        // Remove spaces and special characters, take first 8 characters, convert to uppercase
        var code = new string(tenantName
            .Where(char.IsLetterOrDigit)
            .Take(8)
            .ToArray())
            .ToUpperInvariant();

        return string.IsNullOrEmpty(code) ? "TENANT" : code;
    }

    #endregion
}
