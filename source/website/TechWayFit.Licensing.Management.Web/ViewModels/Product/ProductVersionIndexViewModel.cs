using System.ComponentModel.DataAnnotations;

namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// ViewModel for the Product Version Index page
/// </summary>
public class ProductVersionIndexViewModel
{
    public Guid ProductId { get; set; }
    
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;
    
    [Display(Name = "Product Description")]
    public string ProductDescription { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for displaying version information in lists and cards
/// </summary>
public class VersionInfoViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    
    [Display(Name = "Version")]
    public string Version { get; set; } = string.Empty;
    
    [Display(Name = "Version Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; }
    
    [Display(Name = "End of Life Date")]
    public DateTime? EndOfLifeDate { get; set; }
    
    [Display(Name = "Support End Date")]
    public DateTime? SupportEndDate { get; set; }
    
    [Display(Name = "Release Notes")]
    public string ReleaseNotes { get; set; } = string.Empty;
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Is Current")]
    public bool IsCurrent { get; set; } = false;
    
    [Display(Name = "Can Delete")]
    public bool CanDelete { get; set; } = true;
}

/// <summary>
/// ViewModel for displaying detailed version information on the details page
/// </summary>
public class ProductVersionDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;
    
    [Display(Name = "Product Description")]
    public string ProductDescription { get; set; } = string.Empty;
    
    [Display(Name = "Version")]
    public string Version { get; set; } = string.Empty;
    
    [Display(Name = "Version Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; }
    
    [Display(Name = "End of Life Date")]
    public DateTime? EndOfLifeDate { get; set; }
    
    [Display(Name = "Release Notes")]
    public string ReleaseNotes { get; set; } = string.Empty;
    
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Is Current")]
    public bool IsCurrent { get; set; } = false;
    
    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }
    
    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }
    
    [Display(Name = "Applicable Features")]
    public List<VersionFeatureViewModel> ApplicableFeatures { get; set; } = new();
}

/// <summary>
/// ViewModel for features applicable to a specific version
/// </summary>
public class VersionFeatureViewModel
{
    public Guid FeatureId { get; set; }
    
    [Display(Name = "Feature Name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;
    
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "Is Enabled")]
    public bool IsEnabled { get; set; } = true;
    
    [Display(Name = "Introduced in Version")]
    public string IntroducedInVersion { get; set; } = string.Empty;
    
    [Display(Name = "Support From Version")]
    public string SupportFromVersion { get; set; } = string.Empty;
    
    [Display(Name = "Support To Version")]
    public string SupportToVersion { get; set; } = string.Empty;
}
