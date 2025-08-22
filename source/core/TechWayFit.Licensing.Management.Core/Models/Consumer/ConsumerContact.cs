using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

/// <summary>
/// Extended contact management for consumers - allows multiple contacts per consumer
/// This is an addon feature that doesn't affect existing ConsumerAccount structure
/// </summary>
public class ConsumerContact
{
    public ConsumerContact()
    {
        Audit.IsActive = true;
    }

    /// <summary>
    /// Unique identifier for the consumer contact
    /// </summary>
    public Guid ContactId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the consumer this contact belongs to
    /// </summary>
    public Guid ConsumerId { get; set; }

    /// <summary>
    /// Tenant identifier for multi-tenant isolation
    /// </summary>
    public Guid TenantId { get; set; } = Guid.Empty;

    /// <summary>
    /// Contact Name
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Contact Email
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Contact Phone
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Contact Address
    /// </summary>
    public string ContactAddress { get; set; } = string.Empty;

    /// <summary>
    /// Company Division (e.g., IT, Sales, Marketing, etc.)
    /// </summary>
    public string CompanyDivision { get; set; } = string.Empty;

    /// <summary>
    /// Contact Designation (e.g., Manager, Director, Developer, etc.)
    /// </summary>
    public string ContactDesignation { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is the primary contact for this division/purpose
    /// </summary>
    public bool IsPrimaryContact { get; set; } = false;

    /// <summary>
    /// Contact type/role (e.g., Technical, Billing, Administrative)
    /// </summary>
    public string ContactType { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about this contact
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Audit information for the consumer contact
    /// </summary>
    public AuditInfo Audit { get; set; } = new();

    /// <summary>
    /// Navigation property to the consumer account
    /// </summary>
    public virtual ConsumerAccount? Consumer { get; set; }
}
