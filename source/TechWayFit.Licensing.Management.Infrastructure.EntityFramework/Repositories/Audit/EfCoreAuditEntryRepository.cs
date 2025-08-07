using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Audit;

/// <summary>
/// Audit entry repository implementation
/// </summary>
public class EfCoreAuditEntryRepository :  BaseRepository<AuditEntry,AuditEntryEntity>, IAuditEntryRepository
{
    public EfCoreAuditEntryRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }
    public async Task<IEnumerable<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId)
    {
        var result = await _dbSet.Where(a => a.EntityType == entityType && a.Id == entityId)
                         .OrderByDescending(a => a.CreatedOn)
                         .ToListAsync();
        return result.Select(a => a.Map());
    }

    public async Task<IEnumerable<AuditEntry>> GetRecentEntriesAsync(int count = 100)
    {
        var result = await _dbSet.OrderByDescending(a => a.CreatedOn)
                         .Take(count)
                         .ToListAsync();
        return result.Select(a => a.Map());
    }
}
