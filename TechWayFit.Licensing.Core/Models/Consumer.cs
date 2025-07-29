namespace TechWayFit.Licensing.Core.Models;

/// <summary>
/// Consumer model for license management system
/// </summary>
public class Consumer
{
    /// <summary>
    /// Unique identifier for the consumer
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;

    /// <summary>
    /// Organization name
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Primary contact person name
    /// </summary>
    public string ContactPerson { get; set; } = string.Empty;

    /// <summary>
    /// Primary contact email
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Secondary contact person name
    /// </summary>
    public string? SecondaryContactPerson { get; set; }

    /// <summary>
    /// Secondary contact email
    /// </summary>
    public string? SecondaryContactEmail { get; set; }

    /// <summary>
    /// Full address as a formatted string
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the consumer is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the consumer was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
