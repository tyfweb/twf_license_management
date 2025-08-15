using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// Enhanced Product Feature View Model for separate controller operations
/// </summary>
public class ProductFeatureCreateEditViewModel
{
    public Guid FeatureId { get; set; }
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid TierId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Feature Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Feature Code")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Is Enabled")]
    public bool IsEnabled { get; set; } = true;

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Support From Version")]
    public Guid? SupportFromVersionId { get; set; }

    [Display(Name = "Support To Version")]
    public Guid? SupportToVersionId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}
