using TechWayFit.Licensing.Management.Core.Models.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Common; 

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;

/// <summary>
/// Repository interface for AuditEntry entities
/// </summary>
public interface IAuditEntryRepository : IDataRepository<AuditEntry>
{
     Task<IEnumerable<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId);
     Task<IEnumerable<AuditEntry>> GetRecentEntriesAsync(int count = 100);
}
