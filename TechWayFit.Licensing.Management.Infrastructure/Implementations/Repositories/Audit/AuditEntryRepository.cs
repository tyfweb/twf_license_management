using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Infrastructure.Data.Context;
using TechWayFit.Licensing.Infrastructure.Data.Repositories;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace TechWayFit.Licensing.Infrastructure.Implementations.Repositories.Audit;

/// <summary>
/// Audit entry repository implementation
/// </summary>
public class AuditEntryRepository : BaseRepository<AuditEntryEntity>, IAuditEntryRepository
{
    public AuditEntryRepository(LicensingDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<AuditEntryEntity>> GetByEntityAsync(string entityType, string entityId)
    {
        return await _dbSet.Where(a => a.EntityType == entityType && a.EntityId == entityId)
                         .OrderByDescending(a => a.CreatedOn)
                         .ToListAsync();
    }

    public async Task<IEnumerable<AuditEntryEntity>> GetRecentEntriesAsync(int count = 100)
    {
        return await _dbSet.OrderByDescending(a => a.CreatedOn)
                         .Take(count)
                         .ToListAsync();
    }
}
