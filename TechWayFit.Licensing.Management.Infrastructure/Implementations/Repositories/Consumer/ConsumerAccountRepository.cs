using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Data.Context;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Repositories.Consumer;

/// <summary>
/// Consumer account repository implementation
/// </summary>
public class ConsumerAccountRepository : BaseRepository<ConsumerAccountEntity>, IConsumerAccountRepository
{
    public ConsumerAccountRepository(LicensingDbContext context) : base(context)
    {
    }
    protected override IQueryable<ConsumerAccountEntity> SearchQuery(IQueryable<ConsumerAccountEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(p => p.CompanyName.Contains(searchQuery) ||
                               p.PrimaryContactName.Contains(searchQuery) ||
                               p.PrimaryContactEmail.Contains(searchQuery));
    }
}
