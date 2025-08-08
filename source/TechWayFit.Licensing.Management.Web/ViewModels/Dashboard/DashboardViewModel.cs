using TechWayFit.Licensing.Management.Web.Models;
using TechWayFit.Licensing.Management.Web.ViewModels.License;
using TechWayFit.Licensing.Management.Web.ViewModels.Approval;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public List<ProductSummaryViewModel> Products { get; set; } = new();
        public LicenseGenerationViewModel LicenseGenerationModel { get; set; } = new();
        public DashboardStatsViewModel Stats { get; set; } = new();
        
        // Dashboard metrics properties
        public int TotalLicenses => Stats.TotalLicenses;
        public int ActiveLicenses { get; set; }
        public int ExpiringLicenses => Stats.ExpiringLicenses;
        public int TotalConsumers => Stats.TotalConsumers;
        
        // Recent activity
        public List<LicenseItemViewModel> RecentLicenses { get; set; } = new();
        
        // Workflow - pending approvals
        public bool ShowPendingApprovals { get; set; } = true;
        public int PendingApprovalsMaxItems { get; set; } = 5;
    }

    public class ProductSummaryViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ConsumerCount { get; set; }
        public int LicenseCount { get; set; }
        public int ExpiringLicenses { get; set; }
    }

    public class DashboardStatsViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalConsumers { get; set; }
        public int TotalLicenses { get; set; }
        public int ExpiringLicenses { get; set; }
        public int ActiveProducts { get; set; }
    }
}
