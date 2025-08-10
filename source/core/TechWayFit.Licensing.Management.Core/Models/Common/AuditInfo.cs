namespace TechWayFit.Licensing.Management.Core.Models.Common;

/// <summary>
/// Composition object for audit information
/// </summary>
public class AuditInfo
{
    /// <summary>
    /// User who created the record
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated the record
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// User who deleted the record
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was deleted
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Indicates if the record is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the record is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Concurrency control
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
