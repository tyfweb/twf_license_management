using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Common;
using TechWayFit.Licensing.Management.Core.Models.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

/// <summary>
/// Database entity for ConsumerAccount
/// </summary>
[Table("consumer_accounts")]
public class ConsumerAccountEntity : AuditEntity, IEntityMapper<ConsumerAccount, ConsumerAccountEntity>
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

    #region IEntityMapper implementation
    public ConsumerAccountEntity Map(ConsumerAccount model)
    {
        if (model == null) return null!;

        return new ConsumerAccountEntity
        {
            Id = model.ConsumerId,
            CompanyName = model.CompanyName,
            AccountCode = model.AccountCode,
            PrimaryContactName = model.PrimaryContact.Name,
            PrimaryContactEmail = model.PrimaryContact.Email,
            PrimaryContactPhone = model.PrimaryContact.Phone,
            PrimaryContactPosition = model.PrimaryContact.Position,
            SecondaryContactName = model.SecondaryContact?.Name,
            SecondaryContactEmail = model.SecondaryContact?.Email,
            SecondaryContactPhone = model.SecondaryContact?.Phone,
            SecondaryContactPosition = model.SecondaryContact?.Position,
            ActivatedAt = model.ActivatedAt,
            SubscriptionEnd = model.SubscriptionEnd,
            IsActive = model.Audit.IsActive,
            IsDeleted = model.Audit.IsDeleted,
            AddressStreet = model.Address.Street,
            AddressCity = model.Address.City,
            AddressState = model.Address.State,
            AddressPostalCode = model.Address.PostalCode,
            AddressCountry = model.Address.Country,
            Notes = model.Notes,
            Status = model.Status.ToString(),
            CreatedBy = model.Audit.CreatedBy,
            CreatedOn = model.Audit.CreatedOn,
            UpdatedBy = model.Audit.UpdatedBy,
            UpdatedOn = model.Audit.UpdatedOn,
            DeletedBy = model.Audit.DeletedBy,
            DeletedOn = model.Audit.DeletedOn,
            RowVersion = model.Audit.RowVersion
        };
    }

    /// <summary>
    /// Converts ConsumerAccountEntity to ConsumerAccount core model
    /// </summary>
    public ConsumerAccount Map()
    {

        return new ConsumerAccount
        {
            ConsumerId = this.Id,
            CompanyName = this.CompanyName,
            AccountCode = this.AccountCode,
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
            SubscriptionEnd = this.SubscriptionEnd,
            Audit = new AuditInfo
            {
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                UpdatedBy = this.UpdatedBy,
                UpdatedOn = this.UpdatedOn,
                DeletedBy = this.DeletedBy,
                DeletedOn = this.DeletedOn,
                RowVersion = this.RowVersion
            },
            Address = new Address
            {
                Street = this.AddressStreet,
                City = this.AddressCity,
                State = this.AddressState,
                PostalCode = this.AddressPostalCode,
                Country = this.AddressCountry
            },
            Notes = this.Notes,
            Status = Enum.TryParse<ConsumerStatus>(this.Status, out var status) ? status : ConsumerStatus.Prospect
        };
    }
    #endregion
}
