namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;

public partial class PostgreSqlPostgreSqlLicensingDbContext
{
    /// <summary>
    /// Scoped helper to temporarily disable tenant filtering
    /// </summary>
    private class TenantFilterScope : IDisposable
    {
        public void Dispose()
        {
            // Implementation for restoring filters would go here
            // For now, this is a placeholder for the pattern
        }
    }

   
}
