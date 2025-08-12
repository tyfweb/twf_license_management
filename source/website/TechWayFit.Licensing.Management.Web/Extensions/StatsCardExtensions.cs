using TechWayFit.Licensing.Management.Web.ViewModels.Shared;

namespace TechWayFit.Licensing.Management.Web.Extensions;

/// <summary>
/// Extension methods for creating statistics cards
/// </summary>
public static class StatsCardExtensions
{
    /// <summary>
    /// Creates a basic stats card
    /// </summary>
    public static StatsCardViewModel CreateStatsCard(
        string value, 
        string label, 
        string iconClass = "fas fa-info-circle",
        string? cardClass = null,
        string columnClass = "col-sm-6 col-lg-3",
        string? numberClass = null,
        string? tooltip = null,
        string? actionUrl = null)
    {
        return new StatsCardViewModel
        {
            Value = value,
            Label = label,
            IconClass = iconClass,
            CardClass = cardClass ?? string.Empty,
            ColumnClass = columnClass,
            NumberClass = numberClass ?? string.Empty,
            Tooltip = tooltip,
            ActionUrl = actionUrl
        };
    }

    /// <summary>
    /// Creates a success-styled stats card
    /// </summary>
    public static StatsCardViewModel CreateSuccessStatsCard(
        string value, 
        string label, 
        string iconClass = "fas fa-check-circle",
        string columnClass = "col-sm-6 col-lg-3",
        string? numberClass = null,
        string? tooltip = null,
        string? actionUrl = null)
    {
        return CreateStatsCard(value, label, iconClass, "stats-card-success", columnClass, numberClass, tooltip, actionUrl);
    }

    /// <summary>
    /// Creates a warning-styled stats card
    /// </summary>
    public static StatsCardViewModel CreateWarningStatsCard(
        string value, 
        string label, 
        string iconClass = "fas fa-exclamation-triangle",
        string columnClass = "col-sm-6 col-lg-3",
        string? numberClass = null,
        string? tooltip = null,
        string? actionUrl = null)
    {
        return CreateStatsCard(value, label, iconClass, "stats-card-warning", columnClass, numberClass, tooltip, actionUrl);
    }

    /// <summary>
    /// Creates a danger-styled stats card
    /// </summary>
    public static StatsCardViewModel CreateDangerStatsCard(
        string value, 
        string label, 
        string iconClass = "fas fa-times-circle",
        string columnClass = "col-sm-6 col-lg-3",
        string? numberClass = null,
        string? tooltip = null,
        string? actionUrl = null)
    {
        return CreateStatsCard(value, label, iconClass, "stats-card-danger", columnClass, numberClass, tooltip, actionUrl);
    }

    /// <summary>
    /// Creates an info-styled stats card
    /// </summary>
    public static StatsCardViewModel CreateInfoStatsCard(
        string value, 
        string label, 
        string iconClass = "fas fa-info-circle",
        string columnClass = "col-sm-6 col-lg-3",
        string? numberClass = null,
        string? tooltip = null,
        string? actionUrl = null)
    {
        return CreateStatsCard(value, label, iconClass, "stats-card-info", columnClass, numberClass, tooltip, actionUrl);
    }
}
