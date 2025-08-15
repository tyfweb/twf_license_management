using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// Product Version View Model for Create and Edit operations
/// </summary>
public class ProductVersionCreateEditViewModel
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Version Number")]
    public string Version { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Version Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Release Notes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [Display(Name = "Release Date")]
    public DateTime? ReleaseDate { get; set; }

    [Display(Name = "Support End Date")]
    public DateTime? SupportEndDate { get; set; }

    [Display(Name = "End of Life Date")]
    public DateTime? EndOfLifeDate { get; set; }

    [Display(Name = "Is Current Version")]
    public bool IsCurrent { get; set; } = false;

    public bool IsActive { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}
