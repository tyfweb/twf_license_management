namespace TechWayFit.Licensing.Management.Core.Models.Report;

/// <summary>
/// Report grouping options
/// </summary>
public enum ReportGroupBy
{
    Day,
    Week,
    Month,
    Quarter,
    Year
}

/// <summary>
/// Date range options
/// </summary>
public enum DateRange
{
    Today,
    Yesterday,
    Last7Days,
    Last30Days,
    LastQuarter,
    LastYear,
    Custom
}

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
    PDF,
    Excel,
    CSV,
    JSON
}

/// <summary>
/// Compliance type options
/// </summary>
public enum ComplianceType
{
    All,
    LicenseCompliance,
    SecurityCompliance,
    AuditCompliance,
    DataRetention
}

/// <summary>
/// Violation type options
/// </summary>
public enum ViolationType
{
    UnauthorizedUsage,
    ExceededLimits,
    ExpiredLicense,
    InvalidActivation,
    TamperingDetected
}
