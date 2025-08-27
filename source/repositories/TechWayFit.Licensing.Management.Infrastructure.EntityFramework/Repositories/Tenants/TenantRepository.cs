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
        private readonly EfCoreLicensingDbContext _context;
        private readonly IUserContext _userContext;

        public EfCoreTenantRepository(EfCoreLicensingDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
            _tenants = context.Set<TenantEntity>();
        }

        public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _tenants.FindAsync(id);
            if (entity == null) 
                return false;

            entity.IsActive = true;
            entity.UpdatedBy = _userContext.UserName ?? "System";
            entity.UpdatedOn = DateTime.UtcNow;

            _tenants.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Tenant> AddAsync(Tenant entity, CancellationToken cancellationToken = default)
        {
            var tenantEntity = new TenantEntity();
            tenantEntity.Map(entity);
            tenantEntity.IsActive = true;
            tenantEntity.CreatedBy = _userContext.UserName ?? "System";
            tenantEntity.CreatedOn = DateTime.UtcNow;
            
            _tenants.Add(tenantEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return tenantEntity.Map();
        }

        public async Task<Tenant?> CreateTenantAsync(Guid tenantId, string tenantName, string? tenantDescription = null, CancellationToken cancellationToken = default)
        {
            // Check if tenant already exists
            if (await ExistsAsync(tenantId, true, cancellationToken))
            {
                return null; // Tenant already exists
            }

            var tenant = new Tenant
            {
                TenantId = tenantId,
                TenantName = tenantName,
                Description = tenantDescription,
                TenantCode = GenerateTenantCode(tenantName)
            };

            return await AddAsync(tenant, cancellationToken);
        }

        private string GenerateTenantCode(string tenantName)
        {
            // Generate a unique tenant code based on tenant name
            var cleanName = tenantName.Replace(" ", "").Replace("-", "").Replace("_", "");
            var code = cleanName.Length > 8 ? cleanName.Substring(0, 8).ToUpperInvariant() : cleanName.ToUpperInvariant();
            return $"{code}_{DateTime.UtcNow:yyyyMM}";
        }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenants.FindAsync(id);
        if (tenant == null)
            return false;

        tenant.IsActive = false;
        tenant.UpdatedBy = _userContext.UserName ?? "System";
        tenant.UpdatedOn = DateTime.UtcNow;

        _tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenants.FindAsync(id);
            if (tenant == null)
                return false;

            tenant.IsDeleted = true;
            tenant.UpdatedBy = _userContext.UserName ?? "System";
            tenant.UpdatedOn = DateTime.UtcNow;

            _tenants.Update(tenant);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var query = _tenants.AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(t => !t.IsDeleted);
            }
            
            return await query.AnyAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Tenant?> FindOneAsync(SearchRequest<Tenant> request, CancellationToken cancellationToken = default)
        {
            var query = _tenants.AsQueryable();
            
            // Apply basic filters
            query = query.Where(t => !t.IsDeleted);
            
            // You can expand this to use the SearchRequest filters
            // For now, implementing basic functionality
            
            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            return entity?.Map();
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

        public async Task<bool> UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
        {
            var existingEntity = await _tenants.FindAsync(entity.TenantId);
            if (existingEntity == null)
                return false;

            // Map the updated values
            existingEntity.Map(entity);
            existingEntity.UpdatedBy = _userContext.UserName ?? "System";
            existingEntity.UpdatedOn = DateTime.UtcNow;

            _tenants.Update(existingEntity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}