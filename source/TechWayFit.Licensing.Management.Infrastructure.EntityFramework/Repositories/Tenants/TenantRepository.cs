using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Tenants;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Tenants
{
    public class EfCoreTenantRepository : ITenantRepository
    {
        private readonly DbSet<TenantEntity> _tenants;
        public EfCoreTenantRepository(EfCoreLicensingDbContext context, IUserContext userContext)
        {
            _tenants = context.Set<TenantEntity>();
        }

        public Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = _tenants.Find(id);
            if (entity == null) return Task.FromResult(false);

            entity.IsActive = true;
            return Task.FromResult(true);
        }

        public Task<Tenant> AddAsync(Tenant entity, CancellationToken cancellationToken = default)
        {
            var tenantEntity = new TenantEntity();
            tenantEntity.Map(entity);
            tenantEntity.IsActive = true;
            
            _tenants.Add(tenantEntity);
            return Task.FromResult(tenantEntity.Map());
        }

        public Task<Tenant?> CreateTenantAsync(Guid tenantId, string tenantName, string? tenantDescription = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Tenant?> FindOneAsync(SearchRequest<Tenant> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _tenants.AsNoTracking().ToListAsync(cancellationToken);
            return entities.Select(entity => entity.Map());
        }

        public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _tenants.FindAsync(id);
            if (entity == null) return null;

            return entity.Map();
        }

        public Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}