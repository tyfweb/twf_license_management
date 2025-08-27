using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Consumer;

/// <summary>
/// PostgreSQL implementation of Consumer Account repository
/// </summary>
public class EfCoreConsumerAccountRepository : BaseRepository<ConsumerAccount,ConsumerAccountEntity>, IConsumerAccountRepository
{
    public EfCoreConsumerAccountRepository(EfCoreLicensingDbContext context,IUserContext userContext) : base(context,userContext)
    {
    }

    public async Task<ConsumerAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var entity = await _dbSet
            .FirstOrDefaultAsync(c => 
                c.PrimaryContactEmail == email || 
                (c.SecondaryContactEmail != null && c.SecondaryContactEmail == email),
                cancellationToken);

        return entity?.Map();
    }

    protected override IQueryable<ConsumerAccountEntity> SearchQuery(IQueryable<ConsumerAccountEntity> query, string searchQuery)
    {
        return base.SearchQuery(query, searchQuery)
                   .Where(p => p.CompanyName.Contains(searchQuery) ||
                               p.PrimaryContactName.Contains(searchQuery) ||
                               p.PrimaryContactEmail.Contains(searchQuery));
    }
}
