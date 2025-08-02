using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;

using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Audit;

/// <summary>
/// Audit entry repository implementation
/// </summary>
public class PostgreSqlAuditEntryRepository : PostgreSqlBaseRepository<AuditEntryEntity>, IAuditEntryRepository
{
    public PostgreSqlAuditEntryRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
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
