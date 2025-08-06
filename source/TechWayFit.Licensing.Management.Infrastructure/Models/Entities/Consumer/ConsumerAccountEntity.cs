using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

/// <summary>
/// Database entity for ConsumerAccount
/// </summary>
[Table("consumer_accounts")]
public class ConsumerAccountEntity : AuditEntity
{
    
    /// <summary>
    ///  Company or organization name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Account code for the consumer
    /// </summary>
    public string? AccountCode { get; set; }
    
    /// <summary>
    /// Primary contact information for the consumer
    /// </summary>
    public string PrimaryContactName { get; set; } = string.Empty;
    
    public string PrimaryContactEmail { get; set; } = string.Empty;
    
    public string PrimaryContactPhone { get; set; } = string.Empty;
    
    public string PrimaryContactPosition { get; set; } = string.Empty;
    
    /// <summary>
    /// Secondary contact information for the consumer (optional)
    /// </summary>
    public string? SecondaryContactName { get; set; }
    
    public string? SecondaryContactEmail { get; set; }
    
    public string? SecondaryContactPhone { get; set; }
    
    public string? SecondaryContactPosition { get; set; }

    /// <summary>
    /// Date when the consumer was activated
    /// </summary>
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Subscription end date
    /// </summary>
    public DateTime? SubscriptionEnd { get; set; }
    
    /// <summary>
    /// Indicates if the consumer is active
    /// </summary>
    public new bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Address of the consumer
    /// </summary>
    public string AddressStreet { get; set; } = string.Empty;
    
    public string AddressCity { get; set; } = string.Empty;
    
    public string AddressState { get; set; } = string.Empty;
    
    public string AddressPostalCode { get; set; } = string.Empty;
    
    public string AddressCountry { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional notes or comments about the consumer
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the consumer account
    /// </summary>
    public string? Status { get; set; }

    
    public ICollection<ProductLicenseEntity> Licenses { get; set; } = new List<ProductLicenseEntity>(); 
    public ICollection<ProductConsumerEntity> ProductConsumers { get; set; } = new List<ProductConsumerEntity>();
}
