using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Backup.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Backup.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Backup.Repositories.Consumer;

/// <summary>
/// PostgreSQL implementation of Consumer Contact repository
/// </summary>
public class PostgreSqlConsumerContactRepository : BaseRepository<ConsumerContact,ConsumerContactEntity>, IConsumerContactRepository
{
    public PostgreSqlConsumerContactRepository(PostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }
    protected override IQueryable<ConsumerContactEntity> SearchQuery(IQueryable<ConsumerContactEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(p => p.ContactName.Contains(searchQuery) ||
                               p.ContactEmail.Contains(searchQuery) ||
                               p.ContactPhone.Contains(searchQuery) ||
                               p.CompanyDivision.Contains(searchQuery) ||
                               p.ContactDesignation.Contains(searchQuery));
    }

    public async Task<IEnumerable<ConsumerContact>> GetByConsumerIdAsync(Guid consumerId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<ConsumerContactEntity>()
            .Where(c => c.ConsumerId == consumerId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.Map());
    }
}
