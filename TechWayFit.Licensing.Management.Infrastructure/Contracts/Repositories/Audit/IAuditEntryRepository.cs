using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Audit;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;

/// <summary>
/// Repository interface for AuditEntry entities
/// </summary>
public interface IAuditEntryRepository : IBaseRepository<AuditEntryEntity>
{
     Task<IEnumerable<AuditEntryEntity>> GetByEntityAsync(string entityType, string entityId);
     Task<IEnumerable<AuditEntryEntity>> GetRecentEntriesAsync(int count = 100);
}
