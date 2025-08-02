using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Consumer;

/// <summary>
/// PostgreSQL implementation of Consumer Account repository
/// </summary>
public class PostgreSqlConsumerAccountRepository : PostgreSqlBaseRepository<ConsumerAccountEntity>, IConsumerAccountRepository
{
    public PostgreSqlConsumerAccountRepository(PostgreSqlPostgreSqlLicensingDbContext context) : base(context)
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
