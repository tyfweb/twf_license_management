using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductVersion
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Unique identifier for the product version
    /// </summary>
    public Guid VersionId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Audit information for the product version
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Workflow information for the product version
    /// </summary>
    public WorkflowInfo Workflow { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public bool IsCurrent { get; set; } = false;
    /// <summary>
    /// Version number of the product
    /// </summary>
    public SemanticVersion Version { get; set; } = SemanticVersion.Default;

    /// <summary>
    /// Release date of this version
    /// </summary>
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// End of life date for this version
    /// </summary>
    public DateTime? EndOfLifeDate { get; set; }
    /// <summary>
    /// Support end date for this version
    /// </summary>
    public DateTime? SupportEndDate { get; set; }

    /// <summary>
    /// Description of the changes in this version
    /// </summary>
    public string ChangeLog { get; set; } = string.Empty;

    // IsActive property is inherited from BaseAuditModel
}
