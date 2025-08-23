namespace TechWayFit.Licensing.Management.Web.ViewModels.Product;

/// <summary>
/// ViewModel for managing feature selections in tier view
/// </summary>
public class TierFeatureSelectionViewModel
{
    /// <summary>
    /// Product tier information
    /// </summary>
    public Guid TierId { get; set; }
    public Guid ProductId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string TierDescription { get; set; } = string.Empty;

    /// <summary>
    /// Available features for the product
    /// </summary>
    public List<FeatureSelectionItem> AvailableFeatures { get; set; } = new();

    /// <summary>
    /// Currently selected feature IDs for this tier
    /// </summary>
    public List<Guid> SelectedFeatureIds { get; set; } = new();
}

/// <summary>
/// Represents a feature that can be selected for a tier
/// </summary>
public class FeatureSelectionItem
{
    public Guid FeatureId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsSelected { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? Configuration { get; set; }
}

/// <summary>
/// Request model for updating tier features
/// </summary>
public class UpdateTierFeaturesRequest
{
    public Guid TierId { get; set; }
    public List<Guid> SelectedFeatureIds { get; set; } = new();
}

/// <summary>
/// Enhanced tier view model with feature selection capability
/// </summary>
public class TierWithFeaturesViewModel : ProductTierViewModel
{
    /// <summary>
    /// Features currently mapped to this tier
    /// </summary>
    public List<FeatureSelectionItem> MappedFeatures { get; set; } = new();

    /// <summary>
    /// Total number of features available for the product
    /// </summary>
    public int TotalAvailableFeatures { get; set; } = 0;

    /// <summary>
    /// Number of features currently mapped to this tier
    /// </summary>
    public int MappedFeatureCount => MappedFeatures.Count;
}
