using TechWayFit.Licensing.Core.Models;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product
{
    public class ProductLicenseViewModel
    {
        public ProductConfiguration Product { get; set; } = new();
        public List<ConsumerLicenseViewModel> Consumers { get; set; } = new();
        public ProductLicenseStatsViewModel Stats { get; set; } = new();
    }

    public class ConsumerLicenseViewModel
    {
        public TechWayFit.Licensing.Core.Models.Consumer Consumer { get; set; } = new();
        public List<TechWayFit.Licensing.Core.Models.ConsumerLicenseInfo> Licenses { get; set; } = new();
        public bool HasActiveLicense => Licenses.Any(l => l.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Active && l.ValidTo > DateTime.UtcNow);
        public bool HasExpiringLicense => Licenses.Any(l => l.Status == TechWayFit.Licensing.Core.Models.LicenseStatus.Active && l.ValidTo <= DateTime.UtcNow.AddDays(30) && l.ValidTo > DateTime.UtcNow);
    }

    public class ProductLicenseStatsViewModel
    {
        public int TotalConsumers { get; set; }
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiringLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
    }
}
