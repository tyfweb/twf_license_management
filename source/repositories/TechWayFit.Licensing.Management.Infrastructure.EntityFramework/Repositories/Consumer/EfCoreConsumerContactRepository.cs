using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Consumer;

/// <summary>
/// Entity Framework implementation of Consumer Contact repository
/// </summary>
public class EfCoreConsumerContactRepository : BaseRepository<ConsumerContact, ConsumerContactEntity>, IConsumerContactRepository
{
    public EfCoreConsumerContactRepository(EfCoreLicensingDbContext context, IUserContext userContext) : base(context, userContext)
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
