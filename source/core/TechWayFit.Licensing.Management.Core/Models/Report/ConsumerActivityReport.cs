namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Consumer activity report
/// </summary>
public class ConsumerActivityReport
{
    public DateTime ReportDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IEnumerable<ConsumerActivity> ConsumerActivities { get; set; } = new List<ConsumerActivity>();
}

/// <summary>
/// Consumer activity details
/// </summary>
public class ConsumerActivity
{
    public string ConsumerId { get; set; } = string.Empty;
    public string ConsumerName { get; set; } = string.Empty;
    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public DateTime LastActivity { get; set; }
    public int ValidationAttempts { get; set; }
    public int SuccessfulValidations { get; set; }
    public decimal TotalSpent { get; set; }
}
