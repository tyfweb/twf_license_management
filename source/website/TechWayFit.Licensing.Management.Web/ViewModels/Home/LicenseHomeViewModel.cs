using System;
using TechWayFit.Licensing.Management.Web.ViewModels.Audit;
using TechWayFit.Licensing.Management.Web.ViewModels.License;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Home;

public class LicenseHomeViewModel
{
    public required LicenseStatsViewModel LicenseStats { get; set; }
    public IEnumerable<AuditEntryItemViewModel> RecentAuditLogs { get; set; } = new List<AuditEntryItemViewModel>();
}
