using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductVersion
/// </summary>
[Table("product_versions")]
public class ProductVersionEntity : AuditWorkflowEntity
{

    /// <summary>
    /// Foreign key to Product
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Version number
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Version name or codename
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Release date of this version
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// End of life date for this version
    /// </summary>
    public DateTime? EndOfLifeDate { get; set; }

    public DateTime? SupportEndDate { get; set; }

    /// <summary>
    /// Release notes for this version
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the current version
    /// </summary>
    public bool IsCurrent { get; set; } = false;


    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual ProductEntity? Product { get; set; }
}
