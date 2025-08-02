using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;

/// <summary>
/// Database entity for ProductVersion
/// </summary>
[Table("product_versions")]
public class ProductVersionEntity : BaseAuditEntity
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

    public static ProductVersionEntity FromModel(ProductVersion model)
    {
        return new ProductVersionEntity
        {
            Id = model.VersionId,
            ProductId = model.ProductId,
            Version = model.Version,
            ReleaseDate = model.ReleaseDate,
            EndOfLifeDate = model.EndOfLifeDate,
            SupportEndDate = model.SupportEndDate,
            ReleaseNotes = model.ChangeLog,
            IsCurrent = model.IsCurrent,
            CreatedBy = "system", // Assuming system created
            UpdatedBy = "system", // Assuming system updated
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        };
    }
    public ProductVersion ToModel()
    {
        return new ProductVersion
        {
            VersionId = this.Id,
            ProductId = this.ProductId,
            Version = this.Version,
            ReleaseDate = this.ReleaseDate,
            EndOfLifeDate = this.EndOfLifeDate,
            SupportEndDate = this.SupportEndDate,
            ChangeLog = this.ReleaseNotes,
            IsCurrent = this.IsCurrent
        };
    }
}
