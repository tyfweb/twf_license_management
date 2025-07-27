using TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Common;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Audit;

namespace TechWayFit.Licensing.Infrastructure.Contracts.Repositories.Audit;

/// <summary>
/// Repository interface for AuditEntry entities
/// </summary>
public interface IAuditEntryRepository : IBaseRepository<AuditEntryEntity>
{
     Task<IEnumerable<AuditEntryEntity>> GetByEntityAsync(string entityType, string entityId);
     Task<IEnumerable<AuditEntryEntity>> GetRecentEntriesAsync(int count = 100);
}
