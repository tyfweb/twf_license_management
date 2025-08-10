namespace TechWayFit.Licensing.Management.Web.ViewModels.License
{
    public class LicenseStatsViewModel
    {
        public int TotalConsumers { get; set; }
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiringLicenses { get; set; }
        public int ExpiredLicenses { get; set; }

        public int PendingApprovals { get; set; }
    }
}
