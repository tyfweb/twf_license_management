using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

public abstract class AuditEntity
{
     /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
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
    /// Concurrency control field
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public virtual AuditInfo MapAuditInfo()
    {
        return new AuditInfo
        {
            IsActive = IsActive,
            IsDeleted = IsDeleted,
            CreatedBy = CreatedBy,
            CreatedOn = CreatedOn,
            UpdatedBy = UpdatedBy,
            UpdatedOn = UpdatedOn,
            DeletedBy = DeletedBy,
            DeletedOn = DeletedOn,
            RowVersion = RowVersion
        };
    }
    public virtual void UpdateAuditInfo(AuditInfo auditInfo)
    {
        IsActive = auditInfo.IsActive;
        IsDeleted = auditInfo.IsDeleted;
        CreatedBy = auditInfo.CreatedBy;
        CreatedOn = auditInfo.CreatedOn;
        UpdatedBy = auditInfo.UpdatedBy;
        UpdatedOn = auditInfo.UpdatedOn;
        DeletedBy = auditInfo.DeletedBy;
        DeletedOn = auditInfo.DeletedOn;
        RowVersion = auditInfo.RowVersion;
    }
}
/// <summary>
/// Base entity for audit tracking - contains only audit fields
/// </summary>
public abstract class BaseEntity : AuditEntity
{
   
    /// <summary>
    /// Unique identifier for the tenant
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Indicates if the record is read-only and cannot be modified
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Indicates if the record can be deleted
    /// </summary>
    public bool CanDelete { get; set; } = true;

    


    /// <summary>
    /// Tenant information for multi-tenancy support
    /// </summary>
    public virtual TenantEntity? Tenant { get; set; }
    
    
}
    