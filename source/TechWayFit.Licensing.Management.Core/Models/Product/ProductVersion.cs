using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Product;

public class ProductVersion
{
    public string ProductId { get; set; } = string.Empty;
    /// <summary>
    /// Unique identifier for the product version
    /// </summary>
    public string VersionId { get; set; } = string.Empty;
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
}
