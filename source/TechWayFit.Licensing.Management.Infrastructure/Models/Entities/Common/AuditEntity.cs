using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;

/// <summary>
/// Base entity for audit tracking - contains only audit fields
/// </summary>
public abstract class AuditEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Indicates if the record is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the record is soft deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// User who created the record
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated the record
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// User who deleted the record
    /// </summary>
    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Timestamp when the record was deleted
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Indicates if the record is read-only and cannot be modified
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Indicates if the record can be deleted
    /// </summary>
    public bool CanDelete { get; set; } = true;

    /// <summary>
    /// Concurrency control field
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
