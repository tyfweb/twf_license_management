using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Audit;

/// <summary>
/// Audit entry repository implementation
/// </summary>
public class PostgreSqlAuditEntryRepository :  BaseRepository<AuditEntry,AuditEntryEntity>, IAuditEntryRepository
{
    public PostgreSqlAuditEntryRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
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
