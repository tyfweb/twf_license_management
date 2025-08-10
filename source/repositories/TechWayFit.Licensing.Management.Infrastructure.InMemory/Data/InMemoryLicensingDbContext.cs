using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Data;

/// <summary>
/// InMemory-specific Entity Framework DbContext for the licensing management system
/// This inherits from the EntityFramework DbContext but configures for InMemory database
/// </summary>
public class InMemoryLicensingDbContext : EfCoreLicensingDbContext
{
    public InMemoryLicensingDbContext(DbContextOptions<EfCoreLicensingDbContext> options, IUserContext userContext) 
        : base(options, userContext)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("LicensingInMemoryDb");
        }
        
        base.OnConfiguring(optionsBuilder);
    }
}
