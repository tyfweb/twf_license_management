using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Enums;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.License;

/// <summary>
/// Entity Framework implementation of Product Activation repository
/// </summary>
public class EfCoreProductActivationRepository : BaseRepository<ProductActivation, ProductActivationEntity>, IProductActivationRepository
{
    public EfCoreProductActivationRepository(EfCoreLicensingDbContext context, IUserContext userContext) 
        : base(context, userContext)
    {
    }

    /// <summary>
    /// Gets an activation by its signature
    /// </summary>
    public async Task<ProductActivation?> GetBySignatureAsync(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return null;

        var entity = await _dbSet
            .FirstOrDefaultAsync(a => a.ActivationSignature == signature && !a.IsDeleted);
        
        return entity?.Map();
    }

    /// <summary>
    /// Gets an activation by product key and machine ID
    /// </summary>
    public async Task<ProductActivation?> GetByProductKeyAndMachineAsync(string productKey, string machineId)
    {
        if (string.IsNullOrWhiteSpace(productKey) || string.IsNullOrWhiteSpace(machineId))
            return null;

        var entity = await _dbSet
            .FirstOrDefaultAsync(a => a.ProductKey == productKey && 
                                    a.MachineId == machineId && 
                                    !a.IsDeleted);
        
        return entity?.Map();
    }

    /// <summary>
    /// Gets all activations for a product key
    /// </summary>
    public async Task<IEnumerable<ProductActivation>> GetByProductKeyAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            return Enumerable.Empty<ProductActivation>();

        var entities = await _dbSet
            .Where(a => a.ProductKey == productKey && !a.IsDeleted)
            .OrderByDescending(a => a.ActivationDate)
            .ToListAsync();
            
        return entities.Select(e => e.Map());
    }

    /// <summary>
    /// Gets activation configuration by license ID
    /// </summary>
    public async Task<ProductActivation?> GetConfigurationByLicenseIdAsync(Guid licenseId)
    {
        var entity = await _dbSet
            .FirstOrDefaultAsync(a => a.LicenseId == licenseId && !a.IsDeleted);
            
        return entity?.Map();
    }

    /// <summary>
    /// Counts active activations for a product key
    /// </summary>
    public async Task<int> CountActiveActivationsByProductKeyAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
            return 0;

        return await _dbSet
            .CountAsync(a => a.ProductKey == productKey && 
                           a.Status == ProductActivationStatus.Active && 
                           !a.IsDeleted);
    }

    /// <summary>
    /// Gets activations by license ID
    /// </summary>
    public async Task<IEnumerable<ProductActivation>> GetByLicenseIdAsync(Guid licenseId)
    {
        var entities = await _dbSet
            .Where(a => a.LicenseId == licenseId && !a.IsDeleted)
            .OrderByDescending(a => a.ActivationDate)
            .ToListAsync();
            
        return entities.Select(e => e.Map());
    }

    /// <summary>
    /// Gets activations that haven't sent heartbeat within specified time
    /// </summary>
    public async Task<IEnumerable<ProductActivation>> GetStaleActivationsAsync(TimeSpan timeSpan)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeSpan);

        var entities = await _dbSet
            .Where(a => a.Status == ProductActivationStatus.Active &&
                       (a.LastHeartbeat == null || a.LastHeartbeat < cutoffTime) &&
                       !a.IsDeleted)
            .ToListAsync();
            
        return entities.Select(e => e.Map());
    }

    /// <summary>
    /// Updates heartbeat for an activation
    /// </summary>
    public async Task<bool> UpdateHeartbeatAsync(Guid activationId)
    {
        var activation = await _dbSet.FirstOrDefaultAsync(a => a.Id == activationId && !a.IsDeleted);
        if (activation == null)
            return false;

        activation.LastHeartbeat = DateTime.UtcNow;
        activation.UpdatedOn = DateTime.UtcNow;
        activation.UpdatedBy = _userContext.UserName ?? "SYSTEM";

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets activations by status
    /// </summary>
    public async Task<IEnumerable<ProductActivation>> GetByStatusAsync(ProductActivationStatus status)
    {
        var entities = await _dbSet
            .Where(a => a.Status == status && !a.IsDeleted)
            .OrderByDescending(a => a.ActivationDate)
            .ToListAsync();
            
        return entities.Select(e => e.Map());
    }

    protected override IQueryable<ProductActivationEntity> SearchQuery(IQueryable<ProductActivationEntity> query, string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return query;

        var lowerSearchQuery = searchQuery.ToLowerInvariant();
        
        return query.Where(a => 
            a.ProductKey.ToLower().Contains(lowerSearchQuery) ||
            a.MachineId.ToLower().Contains(lowerSearchQuery) ||
            (a.MachineName != null && a.MachineName.ToLower().Contains(lowerSearchQuery)) ||
            (a.ActivationSignature != null && a.ActivationSignature.ToLower().Contains(lowerSearchQuery))
        );
    }
}
