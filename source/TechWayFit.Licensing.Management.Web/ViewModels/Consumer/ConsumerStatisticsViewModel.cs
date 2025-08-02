namespace TechWayFit.Licensing.Management.Web.ViewModels.Consumer
{
    /// <summary>
    /// Consumer statistics view model
    /// </summary>
    public class ConsumerStatisticsViewModel
    {
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public DateTime? FirstLicense { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
