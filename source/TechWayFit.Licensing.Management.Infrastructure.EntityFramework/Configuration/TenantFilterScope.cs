namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Configuration;

public partial class EfCoreLicensingDbContext
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
