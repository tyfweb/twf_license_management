using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Tenants;

public interface ITenantRepository
{
    Task<Tenant?> CreateTenantAsync(Guid tenantId,
        string tenantName,
        string? tenantDescription = null,
        CancellationToken cancellationToken = default);
    Task<Tenant> AddAsync(Tenant entity, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> FindOneAsync(SearchRequest<Tenant> request, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}