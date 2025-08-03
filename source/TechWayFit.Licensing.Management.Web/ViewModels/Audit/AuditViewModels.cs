using TechWayFit.Licensing.Management.Core.Models.Audit;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Main dashboard view model for audit overview
/// </summary>
public class AuditDashboardViewModel
{
    public AuditStatistics Statistics { get; set; } = new();
    public IEnumerable<AuditEntryItemViewModel> RecentEntries { get; set; } = new List<AuditEntryItemViewModel>();
    public IEnumerable<string> AvailableEntityTypes { get; set; } = new List<string>();
    public IEnumerable<string> AvailableActions { get; set; } = new List<string>();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
    public int TotalEntriesToday { get; set; }
    public int TotalEntriesWeek { get; set; }
    public int TotalEntriesMonth { get; set; }
    public IEnumerable<AuditTrendViewModel> ActivityTrends { get; set; } = new List<AuditTrendViewModel>();
}

/// <summary>
/// Filter criteria for audit searches
/// </summary>
public class AuditFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public string? UserName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "Timestamp";
    public string SortOrder { get; set; } = "desc";
}

/// <summary>
/// Audit entries list view model with pagination
/// </summary>
public class AuditListViewModel
{
    public IEnumerable<AuditEntryItemViewModel> Entries { get; set; } = new List<AuditEntryItemViewModel>();
    public AuditFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public IEnumerable<string> AvailableEntityTypes { get; set; } = new List<string>();
    public IEnumerable<string> AvailableActions { get; set; } = new List<string>();
    public int TotalEntries { get; set; }
    public bool HasResults => Entries.Any();
}

/// <summary>
/// Individual audit entry item for lists
/// </summary>
public class AuditEntryItemViewModel
{
    public string EntryId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Reason { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Display helpers
    public string DisplayActionType => ActionType.Replace("_", " ").ToTitleCase();
    public string DisplayEntityType => EntityType.Replace("_", " ").ToTitleCase();
    public string TimestampDisplay => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    public string RelativeTime => GetRelativeTime(Timestamp);
    public bool HasChanges => !string.IsNullOrEmpty(OldValue) || !string.IsNullOrEmpty(NewValue);
    public string ActionBadgeClass => GetActionBadgeClass(ActionType);

    private static string GetRelativeTime(DateTime timestamp)
    {
        var timeSpan = DateTime.UtcNow - timestamp;
        
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} days ago";
        
        return timestamp.ToString("MMM dd, yyyy");
    }

    private static string GetActionBadgeClass(string actionType)
    {
        return actionType.ToLowerInvariant() switch
        {
            string s when s.Contains("create") => "badge-success",
            string s when s.Contains("update") || s.Contains("modify") => "badge-warning",
            string s when s.Contains("delete") || s.Contains("remove") => "badge-danger",
            string s when s.Contains("login") || s.Contains("access") => "badge-info",
            string s when s.Contains("validate") || s.Contains("verify") => "badge-primary",
            _ => "badge-secondary"
        };
    }
}

/// <summary>
/// Detailed audit entry view model
/// </summary>
public class AuditEntryDetailViewModel
{
    public AuditEntryItemViewModel Entry { get; set; } = new();
    public IEnumerable<AuditEntryItemViewModel> RelatedEntries { get; set; } = new List<AuditEntryItemViewModel>();
    public string? FormattedOldValue { get; set; }
    public string? FormattedNewValue { get; set; }
    public IEnumerable<AuditChangeViewModel> Changes { get; set; } = new List<AuditChangeViewModel>();
    public bool HasRelatedEntries => RelatedEntries.Any();
    public string EntityDisplayName => GetEntityDisplayName();

    private string GetEntityDisplayName()
    {
        if (string.IsNullOrEmpty(Entry.EntityType) || string.IsNullOrEmpty(Entry.EntityId))
            return "Unknown Entity";

        return $"{Entry.EntityType} ({Entry.EntityId})";
    }
}

/// <summary>
/// Represents a change within an audit entry
/// </summary>
public class AuditChangeViewModel
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Modified, Removed
    public string PropertyDisplayName => PropertyName.ToTitleCase();
}

/// <summary>
/// Activity trend data for charts
/// </summary>
public class AuditTrendViewModel
{
    public DateTime Date { get; set; }
    public int EntryCount { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string DateLabel => Date.ToString("MMM dd");
}

/// <summary>
/// Export settings for audit data
/// </summary>
public class AuditExportViewModel
{
    public string Format { get; set; } = "csv"; // csv, json, xml
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IncludeMetadata { get; set; } = true;
    public string FileName => $"audit_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{Format}";
}

/// <summary>
/// Audit statistics overview
/// </summary>
public class AuditStatisticsViewModel
{
    public int TotalEntries { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueEntities { get; set; }
    public IEnumerable<KeyValuePair<string, int>> TopActions { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<string, int>> TopUsers { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<string, int>> TopEntityTypes { get; set; } = new List<KeyValuePair<string, int>>();
    public IEnumerable<KeyValuePair<DateTime, int>> DailyActivity { get; set; } = new List<KeyValuePair<DateTime, int>>();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
}

/// <summary>
/// Pagination helper
/// </summary>
public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public int StartIndex => Math.Max(1, CurrentPage - 2);
    public int EndIndex => Math.Min(TotalPages, CurrentPage + 2);
    public IEnumerable<int> PageNumbers => Enumerable.Range(StartIndex, EndIndex - StartIndex + 1);
}

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

/// <summary>
/// Entity-specific audit view model
/// </summary>
public class EntityAuditViewModel
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; }
    public string EntityDisplayName { get; set; } = string.Empty;
    public IEnumerable<AuditEntryItemViewModel> Entries { get; set; } = new List<AuditEntryItemViewModel>();
    public PaginationViewModel Pagination { get; set; } = new();
    public AuditStatisticsViewModel Statistics { get; set; } = new();
    public DateTime? DateRangeStart { get; set; }
    public DateTime? DateRangeEnd { get; set; }
}

// Extension method for string title case conversion
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(word => 
            char.ToUpperInvariant(word[0]) + (word.Length > 1 ? word[1..].ToLowerInvariant() : "")));
    }
}
