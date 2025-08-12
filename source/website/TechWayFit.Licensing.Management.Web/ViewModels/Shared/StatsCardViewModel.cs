namespace TechWayFit.Licensing.Management.Web.ViewModels.Shared;

/// <summary>
/// View model for reusable statistics cards
/// </summary>
public class StatsCardViewModel
{
    /// <summary>
    /// The display value for the card (can be text or number)
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// The label/title for the card
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// FontAwesome icon class (e.g., "fas fa-building")
    /// </summary>
    public string IconClass { get; set; } = "fas fa-info-circle";
    
    /// <summary>
    /// CSS class for the card styling (e.g., "stats-card-success")
    /// </summary>
    public string CardClass { get; set; } = string.Empty;
    
    /// <summary>
    /// CSS class for the column width (e.g., "col-sm-6 col-lg-3")
    /// </summary>
    public string ColumnClass { get; set; } = "col-sm-6 col-lg-3";
    
    /// <summary>
    /// Additional CSS class for the number/value (e.g., "small")
    /// </summary>
    public string NumberClass { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional tooltip text
    /// </summary>
    public string? Tooltip { get; set; }
    
    /// <summary>
    /// Optional click action URL
    /// </summary>
    public string? ActionUrl { get; set; }
}
