using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for individual user slots within a volumetric license
/// Each slot represents one user allocation (numbered 0001-9999)
/// </summary>
[Table("volumetric_user_slots")]
public class VolumetricUserSlotEntity : AuditWorkflowEntity
{
    #region BaseProperties
    /// <summary>
    /// Foreign key to the VolumetricLicense
    /// </summary>
    public Guid VolumetricLicenseId { get; set; }

    /// <summary>
    /// Slot number (1 to 9999)
    /// </summary>
    public int SlotNumber { get; set; }

    /// <summary>
    /// Full user key in XXXX-XXXX-XXXX-0001 format
    /// </summary>
    public string UserKey { get; set; } = string.Empty;
    #endregion

    #region UserInformation
    /// <summary>
    /// User identifier (can be username, email, or custom ID)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Machine ID where this user slot is currently active
    /// </summary>
    public string? MachineId { get; set; }

    /// <summary>
    /// Human-readable machine name
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Machine fingerprint for security verification
    /// </summary>
    public string? MachineFingerprint { get; set; }

    /// <summary>
    /// IP address from which the user is accessing
    /// </summary>
    public string? IpAddress { get; set; }
    #endregion

    #region ActivationTracking
    /// <summary>
    /// First time this slot was activated
    /// </summary>
    public DateTime? FirstActivation { get; set; }

    /// <summary>
    /// Last activity timestamp for this slot
    /// </summary>
    public DateTime? LastActivity { get; set; }

    /// <summary>
    /// Whether this slot is currently active
    /// </summary>
    public bool IsCurrentlyActive { get; set; } = false;

    /// <summary>
    /// Current session start time
    /// </summary>
    public DateTime? CurrentSessionStart { get; set; }

    /// <summary>
    /// Last heartbeat received
    /// </summary>
    public DateTime? LastHeartbeat { get; set; }
    #endregion

    #region DataStorage
    /// <summary>
    /// Activation-specific data (JSON)
    /// </summary>
    public string ActivationData { get; set; } = "{}";

    /// <summary>
    /// Session-specific data (JSON)
    /// </summary>
    public string SessionData { get; set; } = "{}";

    /// <summary>
    /// Application version information
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Operating system information
    /// </summary>
    public string? OperatingSystem { get; set; }
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to the VolumetricLicense
    /// </summary>
    public virtual VolumetricLicenseEntity? VolumetricLicense { get; set; }
    #endregion
}
