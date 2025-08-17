using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Enums;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for tracking license activities, usage, and transactions
/// This entity captures all changeable data related to license usage
/// </summary>
[Table("license_activities")]
public class LicenseActivityEntity : AuditWorkflowEntity
{
    #region BaseProperties
    /// <summary>
    /// Foreign key to the ProductLicense
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Type of activity/transaction
    /// </summary>
    public LicenseActivityType ActivityType { get; set; }

    /// <summary>
    /// Current status of the license at the time of this activity
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the activity
    /// </summary>
    public DateTime ActivityTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or system that performed this activity
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Description of the activity
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata for the activity (JSON)
    /// </summary>
    public string ActivityData { get; set; } = "{}";
    #endregion

    #region ActivationTrackingProperties
    /// <summary>
    /// Machine ID for activation-related activities
    /// </summary>
    public string? MachineId { get; set; }

    /// <summary>
    /// Machine name for activation-related activities
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Machine fingerprint for security tracking
    /// </summary>
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// IP address from which the activity originated
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User ID for volumetric license activities
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Slot number for volumetric license activities (1-9999)
    /// </summary>
    public int? SlotNumber { get; set; }
    #endregion

    #region UsageCounters
    /// <summary>
    /// Current activation count at the time of this activity (Product Key)
    /// </summary>
    public int? CurrentActivations { get; set; }

    /// <summary>
    /// Current active users count at the time of this activity (Volumetric)
    /// </summary>
    public int? CurrentActiveUsers { get; set; }

    /// <summary>
    /// Total allocated users count at the time of this activity (Volumetric)
    /// </summary>
    public int? TotalAllocatedUsers { get; set; }

    /// <summary>
    /// Download count at the time of this activity (License File)
    /// </summary>
    public int? DownloadCount { get; set; }
    #endregion

    #region FileTrackingProperties
    /// <summary>
    /// File name for file-related activities
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// File size in bytes for file-related activities
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// File hash for integrity verification
    /// </summary>
    public string? FileHash { get; set; }
    #endregion

    #region SessionTrackingProperties
    /// <summary>
    /// Session start time for session-based activities
    /// </summary>
    public DateTime? SessionStartTime { get; set; }

    /// <summary>
    /// Session end time for session-based activities
    /// </summary>
    public DateTime? SessionEndTime { get; set; }

    /// <summary>
    /// Session duration in minutes
    /// </summary>
    public int? SessionDurationMinutes { get; set; }

    /// <summary>
    /// Last heartbeat timestamp for active sessions
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to the ProductLicense
    /// </summary>
    public virtual ProductLicenseEntity? License { get; set; }
    #endregion
}
