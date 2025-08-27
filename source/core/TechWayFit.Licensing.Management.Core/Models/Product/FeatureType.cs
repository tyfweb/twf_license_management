namespace TechWayFit.Licensing.Management.Core.Models.Product;

/// <summary>
/// Enumeration of feature types for categorizing product features
/// </summary>
public enum FeatureType
{
    /// <summary>
    /// Core functionality that is essential to the product
    /// </summary>
    Core,
    
    /// <summary>
    /// Premium features that provide enhanced functionality
    /// </summary>
    Premium,
    
    /// <summary>
    /// Add-on features that extend the product capabilities
    /// </summary>
    Addon,
    
    /// <summary>
    /// Integration features that connect with external systems
    /// </summary>
    Integration,
    
    /// <summary>
    /// API features that provide programmatic access
    /// </summary>
    Api,
    
    /// <summary>
    /// Reporting features that provide analytics and insights
    /// </summary>
    Reporting,
    
    /// <summary>
    /// Security features that enhance system protection
    /// </summary>
    Security,
    
    /// <summary>
    /// Administrative features for system management
    /// </summary>
    Administrative
}
