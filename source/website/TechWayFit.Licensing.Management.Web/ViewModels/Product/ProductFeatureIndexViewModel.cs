using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// ViewModel for the Product Feature Index page
/// </summary>
public class ProductFeatureIndexViewModel
{
    public Guid ProductId { get; set; }
    
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;
    
    [Display(Name = "Product Description")]
    public string ProductDescription { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for displaying feature information in lists and cards
/// </summary>
public class FeatureInfoViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid TierId { get; set; }
    
    [Display(Name = "Feature Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Feature Code")]
    public string Code { get; set; } = string.Empty;
    
    [Display(Name = "Is Enabled")]
    public bool IsEnabled { get; set; } = true;
    
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }
    
    [Display(Name = "Support From Version")]
    public string SupportFromVersion { get; set; } = string.Empty;
    
    [Display(Name = "Support To Version")]
    public string SupportToVersion { get; set; } = string.Empty;
    
    [Display(Name = "Can Delete")]
    public bool CanDelete { get; set; } = true;
}
