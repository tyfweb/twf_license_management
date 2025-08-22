namespace TechWayFit.Licensing.Management.Core.Models.Enums;

/// <summary>
/// Enumeration of license validation strategy types
/// </summary>
public enum LicenseValidationStrategyType
{
    /// <summary>
    /// Standard license validation strategy
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Enterprise license validation strategy with enhanced features
    /// </summary>
    Enterprise = 2,

    /// <summary>
    /// Trial license validation strategy with time and feature restrictions
    /// </summary>
    Trial = 3,

    /// <summary>
    /// Developer license validation strategy for development environments
    /// </summary>
    Developer = 4
}
