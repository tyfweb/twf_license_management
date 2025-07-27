using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Infrastructure.Data.Entities.Consumer;

/// <summary>
/// Database entity for ConsumerAccount
/// </summary>
[Table("license_consumers")]
public class ConsumerAccountEntity : BaseAuditEntity
{
    /// <summary>
    /// Unique identifier for the consumer
    /// </summary>
    public string ConsumerId { get; set; } = string.Empty;
    /// <summary>
    ///  Company or organization name
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;
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
    /// Indicates if the consumer is active
    /// </summary>
    public bool IsActive { get; set; } = true;
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


    public static ConsumerAccountEntity FromModel(ConsumerAccount model)
    {
        return new ConsumerAccountEntity
        {
            ConsumerId = model.ConsumerId,
            CompanyName = model.CompanyName,
            PrimaryContactName = model.PrimaryContact.Name,
            PrimaryContactEmail = model.PrimaryContact.Email,
            PrimaryContactPhone = model.PrimaryContact.Phone,
            PrimaryContactPosition = model.PrimaryContact.Position,
            SecondaryContactName = model.SecondaryContact?.Name,
            SecondaryContactEmail = model.SecondaryContact?.Email,
            SecondaryContactPhone = model.SecondaryContact?.Phone,
            SecondaryContactPosition = model.SecondaryContact?.Position,
            ActivatedAt = model.ActivatedAt,
            IsActive = model.IsActive,
            AddressStreet = model.Address.Street,
            AddressCity = model.Address.City,
            AddressState = model.Address.State,
            AddressPostalCode = model.Address.PostalCode,
            AddressCountry = model.Address.Country,
            Notes = model.Notes,
            Status = ToStringEnum(model.Status)
        };
    }
    public ConsumerAccount ToModel()
    {
        return new ConsumerAccount
        {
            ConsumerId = this.ConsumerId,
            CompanyName = this.CompanyName,
            PrimaryContact = new ContactPerson
            {
                Name = this.PrimaryContactName,
                Email = this.PrimaryContactEmail,
                Phone = this.PrimaryContactPhone,
                Position = this.PrimaryContactPosition ?? string.Empty
            },
            SecondaryContact = new ContactPerson
            {
                Name = this.SecondaryContactName ?? string.Empty,
                Email = this.SecondaryContactEmail ?? string.Empty,
                Phone = this.SecondaryContactPhone ?? string.Empty,
                Position = this.SecondaryContactPosition ?? string.Empty
            },
            ActivatedAt = this.ActivatedAt,
            IsActive = this.IsActive,
            Address = new Address
            {
                Street = this.AddressStreet,
                City = this.AddressCity,
                State = this.AddressState,
                PostalCode = this.AddressPostalCode,
                Country = this.AddressCountry
            },
            Notes = this.Notes,
            Status = ToEnum<ConsumerStatus>(this.Status)
        };
    }

}
