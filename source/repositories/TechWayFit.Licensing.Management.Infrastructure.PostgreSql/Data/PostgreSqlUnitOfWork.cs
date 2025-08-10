using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Data;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Data;

/// <summary>
/// PostgreSQL implementation of Unit of Work pattern that inherits from EntityFramework implementation
/// </summary>
public class PostgreSqlUnitOfWork : EfCoreUnitOfWork
{
    public PostgreSqlUnitOfWork(
        EfCoreLicensingDbContext context, 
        IUserContext userContext) 
        : base(context, userContext)
    {
    }
}
