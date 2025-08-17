using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.License;

/// <summary>
/// Database entity for managing volumetric licenses with multiple user slots
/// This entity handles enterprise licenses that support multiple concurrent users
/// </summary>
[Table("volumetric_licenses")]
public class VolumetricLicenseEntity : AuditWorkflowEntity
{
    #region BaseProperties
    /// <summary>
    /// Foreign key to the ProductLicense
    /// </summary>
    public Guid LicenseId { get; set; }

    /// <summary>
    /// Base volumetric key in XXXX-XXXX-XXXX format - immutable after creation
    /// </summary>
    public string BaseVolumetricKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of concurrent users allowed - immutable after creation
    /// </summary>
    public int MaxConcurrentUsers { get; set; }

    /// <summary>
    /// Maximum total number of users that can be allocated - immutable after creation
    /// </summary>
    public int MaxTotalUsers { get; set; }
    #endregion

    #region CurrentUsageCounters
    /// <summary>
    /// Current number of active users
    /// </summary>
    public int CurrentActiveUsers { get; set; } = 0;

    /// <summary>
    /// Total number of users allocated so far
    /// </summary>
    public int TotalAllocatedUsers { get; set; } = 0;
    #endregion

    #region Configuration
    /// <summary>
    /// Hours after which inactive users are automatically cleaned up
    /// </summary>
    public int AutoCleanupInactiveHours { get; set; } = 24;

    /// <summary>
    /// Additional configuration data (JSON)
    /// </summary>
    public string ConfigurationData { get; set; } = "{}";
    #endregion

    #region SessionManagement
    /// <summary>
    /// Maximum session duration in hours (0 = unlimited)
    /// </summary>
    public int MaxSessionHours { get; set; } = 0;

    /// <summary>
    /// Heartbeat interval required in minutes
    /// </summary>
    public int HeartbeatIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Grace period in minutes before marking a user inactive
    /// </summary>
    public int InactiveGracePeriodMinutes { get; set; } = 15;
    #endregion

    #region NavigationProperties
    /// <summary>
    /// Navigation property to the ProductLicense
    /// </summary>
    public virtual ProductLicenseEntity? License { get; set; }

    /// <summary>
    /// Navigation property to all user slots for this volumetric license
    /// </summary>
    public virtual ICollection<VolumetricUserSlotEntity> UserSlots { get; set; } = new List<VolumetricUserSlotEntity>();
    #endregion
}
