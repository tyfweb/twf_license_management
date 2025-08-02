namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Custom report parameters
/// </summary>
public class CustomReportParameters
{
    public string ReportName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<string> SelectedFields { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public string? GroupBy { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int? MaxRecords { get; set; }
}

/// <summary>
/// Custom report data
/// </summary>
public class CustomReportData
{
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public CustomReportParameters Parameters { get; set; } = new();
    public IEnumerable<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();
    public int TotalRecords { get; set; }
}
