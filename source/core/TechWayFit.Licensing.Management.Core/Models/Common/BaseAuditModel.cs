using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Core.Models.Common;

/// <summary>
/// Base model class that provides audit fields for all core business models
/// </summary>
public abstract class BaseAuditModel : IWorkflowCapable
{
    /// <summary>
    /// Unique identifier for the model (Primary Key)
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Is the model active? This is used for soft deletes
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is the model deleted? This is used for soft deletes
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// User who created the model
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the model was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who last updated the model
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Date and time when the model was last updated
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// User who deleted the model (for soft deletes)
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Date and time when the model was deleted (for soft deletes)
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Workflow status of the entity
    /// </summary>
    public EntityStatus EntityStatus { get; set; } = EntityStatus.Draft;

    /// <summary>
    /// User who submitted the entity for approval
    /// </summary>
    public string? SubmittedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was submitted for approval
    /// </summary>
    public DateTime? SubmittedOn { get; set; }

    /// <summary>
    /// User who approved/rejected the entity
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Date and time when the entity was reviewed
    /// </summary>
    public DateTime? ReviewedOn { get; set; }

    /// <summary>
    /// Approval comments or rejection reason
    /// </summary>
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Audit information composition object - implementation of IWorkflowCapable
    /// </summary>
    public AuditInfo Audit 
    { 
        get => new()
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
        set
        {
            if (value != null)
            {
                IsActive = value.IsActive;
                IsDeleted = value.IsDeleted;
                CreatedBy = value.CreatedBy;
                CreatedOn = value.CreatedOn;
                UpdatedBy = value.UpdatedBy;
                UpdatedOn = value.UpdatedOn;
                DeletedBy = value.DeletedBy;
                DeletedOn = value.DeletedOn;
                RowVersion = value.RowVersion;
            }
        }
    }

    /// <summary>
    /// Workflow information composition object - implementation of IWorkflowCapable
    /// </summary>
    public WorkflowInfo Workflow 
    { 
        get => new()
        {
            Status = EntityStatus,
            SubmittedBy = SubmittedBy,
            SubmittedOn = SubmittedOn,
            ReviewedBy = ReviewedBy,
            ReviewedOn = ReviewedOn,
            ReviewComments = ReviewComments
        };
        set
        {
            if (value != null)
            {
                EntityStatus = value.Status;
                SubmittedBy = value.SubmittedBy;
                SubmittedOn = value.SubmittedOn;
                ReviewedBy = value.ReviewedBy;
                ReviewedOn = value.ReviewedOn;
                ReviewComments = value.ReviewComments;
            }
        }
    }
}

/// <summary>
/// Entity status enum for workflow management
/// </summary>
public enum EntityStatus
{
    /// <summary>
    /// Entity is in draft state - not yet submitted for approval
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Entity has been submitted and is pending approval
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// Entity has been approved and is active
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Entity has been rejected and needs revision
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Entity has been withdrawn from approval process
    /// </summary>
    Withdrawn = 4,

    /// <summary>
    /// Entity is archived (historical record)
    /// </summary>
    Archived = 5
}
