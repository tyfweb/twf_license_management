namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Security audit view model for security-specific entries
/// </summary>
public class SecurityAuditViewModel
{
    public IEnumerable<AuditEntryItemViewModel> SecurityEntries { get; set; } = new List<AuditEntryItemViewModel>();
    public IEnumerable<AuditEntryItemViewModel> FailedLogins { get; set; } = new List<AuditEntryItemViewModel>();
    public IEnumerable<AuditEntryItemViewModel> AccessViolations { get; set; } = new List<AuditEntryItemViewModel>();
    public IEnumerable<AuditEntryItemViewModel> PrivilegeEscalations { get; set; } = new List<AuditEntryItemViewModel>();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    public int TotalSecurityEvents { get; set; }
    public int HighRiskEvents { get; set; }
    public int MediumRiskEvents { get; set; }
    public int LowRiskEvents { get; set; }
}
