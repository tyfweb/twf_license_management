namespace TechWayFit.Licensing.Management.Web.ViewModels.Audit;

/// <summary>
/// Individual audit entry item for lists
/// </summary>
public class AuditEntryItemViewModel
{
    public string EntryId { get; set; } = string.Empty;
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
