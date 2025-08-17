using System.ComponentModel;

namespace TechWayFit.Licensing.Management.Core.Models.Enums;

/// <summary>
/// Enumeration for product activation status
/// </summary>
public enum ProductActivationStatus
{
    /// <summary>
    /// Activation is pending
    /// </summary>
    [Description("Pending Activation")]
    PendingActivation = 0, 

    /// <summary>
    /// Activation is active and valid
    /// </summary>
    [Description("Active")]
    Active = 1,

    /// <summary>
    /// Activation is inactive (not currently in use)
    /// </summary>
    [Description("Inactive")]
    Inactive = 2,

    /// <summary>
    /// Activation has been suspended (temporarily disabled)
    /// </summary>
    [Description("Suspended")]
    Suspended = 3,

    /// <summary>
    /// Activation has expired
    /// </summary>
    [Description("Expired")]
    Expired = 4,

    /// <summary>
    /// Activation has been revoked
    /// </summary>
    [Description("Revoked")]
    Revoked = 5
}
