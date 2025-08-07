using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Consumer;

/// <summary>
/// PostgreSQL implementation of Consumer Account repository
/// </summary>
public class PostgreSqlConsumerAccountRepository : BaseRepository<ConsumerAccount,ConsumerAccountEntity>, IConsumerAccountRepository
{
    public PostgreSqlConsumerAccountRepository(PostgreSqlPostgreSqlLicensingDbContext context,IUserContext userContext) : base(context,userContext)
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
