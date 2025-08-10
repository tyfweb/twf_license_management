using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit
{

    /// <summary>
    /// ViewModel for audit summary and statistics
    /// </summary>
    public class AuditSummaryViewModel
    {
        [Display(Name = "Total Entries")]
        public int TotalEntries { get; set; }

        [Display(Name = "Successful Operations")]
        public int SuccessfulOperations { get; set; }

        [Display(Name = "Failed Operations")]
        public int FailedOperations { get; set; }

        [Display(Name = "Unique Users")]
        public int UniqueUsers { get; set; }

        [Display(Name = "Most Active User")]
        public string? MostActiveUser { get; set; }

        [Display(Name = "Most Active Entity Type")]
        public string? MostActiveEntityType { get; set; }

        [Display(Name = "Date Range")]
        public string DateRange { get; set; } = string.Empty;

        // Charts data
        public Dictionary<string, int> EntriesByAction { get; set; } = new();
        public Dictionary<string, int> EntriesByEntityType { get; set; } = new();
        public Dictionary<string, int> EntriesByUser { get; set; } = new();
        public Dictionary<DateTime, int> EntriesByDate { get; set; } = new();

        [Display(Name = "Success Rate")]
        public double SuccessRate => TotalEntries > 0 ? (double)SuccessfulOperations / TotalEntries * 100 : 0;

        [Display(Name = "Failure Rate")]
        public double FailureRate => TotalEntries > 0 ? (double)FailedOperations / TotalEntries * 100 : 0;
    }
}
