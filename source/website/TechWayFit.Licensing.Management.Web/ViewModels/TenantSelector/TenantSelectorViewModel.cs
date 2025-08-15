namespace TechWayFit.Licensing.Management.Web.ViewModels.TenantSelector;

/// <summary>
/// View model for the tenant selector ViewComponent
/// </summary>
public class TenantSelectorViewModel
{
    /// <summary>
    /// Indicates if the current user is an administrator
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// List of available tenants for selection
    /// </summary>
    public IList<TenantOption> Tenants { get; set; } = new List<TenantOption>();

    /// <summary>
    /// Currently selected tenant ID
    /// </summary>
    public Guid? CurrentTenantId { get; set; }

    /// <summary>
    /// Indicates if there was an error loading tenants
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Error message if there was an error loading tenants
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates if the ViewComponent should be visible
    /// </summary>
    public bool ShouldDisplay => IsAdministrator && !HasError;

    /// <summary>
    /// Gets the display text for the current selected tenant
    /// </summary>
    public string CurrentTenantDisplayText
    {
        get
        {
            if (CurrentTenantId == null)
                return "All Tenants (Admin View)";

            var currentTenant = Tenants.FirstOrDefault(t => t.Id == CurrentTenantId);
            return currentTenant?.DisplayText ?? "Unknown Tenant";
        }
    }
}

/// <summary>
/// Represents a tenant option in the selector
/// </summary>
public class TenantOption
{
    /// <summary>
    /// Tenant unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the dropdown option
    /// </summary>
    public string DisplayText { get; set; } = string.Empty;
}
