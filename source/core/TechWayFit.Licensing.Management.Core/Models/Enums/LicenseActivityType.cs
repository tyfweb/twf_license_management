using System.ComponentModel;

namespace TechWayFit.Licensing.Management.Core.Models.Enums;

/// <summary>
/// Enumeration for different types of license activities and transactions
/// </summary>
public enum LicenseActivityType
{
    // General License Activities
    /// <summary>
    /// License was created
    /// </summary>
    [Description("License Created")]
    LicenseCreated = 1,

    /// <summary>
    /// License was activated
    /// </summary>
    [Description("License Activated")]
    LicenseActivated = 2,

    /// <summary>
    /// License was deactivated
    /// </summary>
    [Description("License Deactivated")]
    LicenseDeactivated = 3,

    /// <summary>
    /// License was revoked
    /// </summary>
    [Description("License Revoked")]
    LicenseRevoked = 4,

    /// <summary>
    /// License was renewed
    /// </summary>
    [Description("License Renewed")]
    LicenseRenewed = 5,

    /// <summary>
    /// License expired
    /// </summary>
    [Description("License Expired")]
    LicenseExpired = 6,

    // Product License File Activities (10-19)
    /// <summary>
    /// License file was generated
    /// </summary>
    [Description("License File Generated")]
    FileGenerated = 10,

    /// <summary>
    /// License file was downloaded
    /// </summary>
    [Description("License File Downloaded")]
    FileDownloaded = 11,

    /// <summary>
    /// License file was validated
    /// </summary>
    [Description("License File Validated")]
    FileValidated = 12,

    /// <summary>
    /// License file validation failed
    /// </summary>
    [Description("License File Validation Failed")]
    FileValidationFailed = 13,

    // Product Key Activities (20-29)
    /// <summary>
    /// Product key was activated on a machine
    /// </summary>
    [Description("Product Key Activated")]
    KeyActivated = 20,

    /// <summary>
    /// Product key was deactivated from a machine
    /// </summary>
    [Description("Product Key Deactivated")]
    KeyDeactivated = 21,

    /// <summary>
    /// Product key validation succeeded
    /// </summary>
    [Description("Product Key Validated")]
    KeyValidated = 22,

    /// <summary>
    /// Product key validation failed
    /// </summary>
    [Description("Product Key Validation Failed")]
    KeyValidationFailed = 23,

    /// <summary>
    /// Heartbeat received from activated machine
    /// </summary>
    [Description("Heartbeat Received")]
    HeartbeatReceived = 24,

    /// <summary>
    /// Machine activation limit exceeded
    /// </summary>
    [Description("Activation Limit Exceeded")]
    ActivationLimitExceeded = 25,

    // Volumetric License Activities (30-39)
    /// <summary>
    /// User slot was allocated
    /// </summary>
    [Description("User Slot Allocated")]
    SlotAllocated = 30,

    /// <summary>
    /// User slot was deallocated
    /// </summary>
    [Description("User Slot Deallocated")]
    SlotDeallocated = 31,

    /// <summary>
    /// User session started
    /// </summary>
    [Description("User Session Started")]
    SessionStarted = 32,

    /// <summary>
    /// User session ended
    /// </summary>
    [Description("User Session Ended")]
    SessionEnded = 33,

    /// <summary>
    /// Concurrent user limit reached
    /// </summary>
    [Description("Concurrent User Limit Reached")]
    ConcurrentLimitReached = 34,

    /// <summary>
    /// Total user limit reached
    /// </summary>
    [Description("Total User Limit Reached")]
    TotalLimitReached = 35,

    /// <summary>
    /// Inactive user sessions cleaned up
    /// </summary>
    [Description("Inactive Sessions Cleaned")]
    InactiveSessionsCleanup = 36,

    // System Activities (40-49)
    /// <summary>
    /// System performed maintenance tasks
    /// </summary>
    [Description("System Maintenance")]
    SystemMaintenance = 40,

    /// <summary>
    /// Audit log entry
    /// </summary>
    [Description("Audit Log")]
    AuditLog = 41,

    /// <summary>
    /// Error occurred during license operation
    /// </summary>
    [Description("Operation Error")]
    OperationError = 42
}
