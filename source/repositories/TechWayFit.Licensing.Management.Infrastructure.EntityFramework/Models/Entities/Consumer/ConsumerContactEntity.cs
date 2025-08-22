using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Consumer;

/// <summary>
/// Database entity for ConsumerContact - Extended contact management
/// </summary>
[Table("consumer_contacts")]
public class ConsumerContactEntity : BaseEntity, IEntityMapper<ConsumerContact, ConsumerContactEntity>
{
    /// <summary>
    /// Reference to the consumer this contact belongs to
    /// </summary>
    [Column("consumer_id")]
    public Guid ConsumerId { get; set; }

    /// <summary>
    /// Contact Name
    /// </summary>
    [Column("contact_name")]
    [MaxLength(200)]
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Contact Email
    /// </summary>
    [Column("contact_email")]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Contact Phone
    /// </summary>
    [Column("contact_phone")]
    [MaxLength(50)]
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Contact Address
    /// </summary>
    [Column("contact_address")]
    [MaxLength(500)]
    public string ContactAddress { get; set; } = string.Empty;

    /// <summary>
    /// Company Division
    /// </summary>
    [Column("company_division")]
    [MaxLength(100)]
    public string CompanyDivision { get; set; } = string.Empty;

    /// <summary>
    /// Contact Designation
    /// </summary>
    [Column("contact_designation")]
    [MaxLength(100)]
    public string ContactDesignation { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this is the primary contact for this division/purpose
    /// </summary>
    [Column("is_primary_contact")]
    public bool IsPrimaryContact { get; set; } = false;

    /// <summary>
    /// Contact type/role
    /// </summary>
    [Column("contact_type")]
    [MaxLength(50)]
    public string ContactType { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about this contact
    /// </summary>
    [Column("notes")]
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the consumer account
    /// </summary>
    public virtual ConsumerAccountEntity? Consumer { get; set; }

    // IEntityMapper implementation
    public ConsumerContactEntity Map(ConsumerContact source)
    {
        Id = source.ContactId;
        ConsumerId = source.ConsumerId;
        TenantId = source.TenantId;
        ContactName = source.ContactName;
        ContactEmail = source.ContactEmail;
        ContactPhone = source.ContactPhone;
        ContactAddress = source.ContactAddress;
        CompanyDivision = source.CompanyDivision;
        ContactDesignation = source.ContactDesignation;
        IsPrimaryContact = source.IsPrimaryContact;
        ContactType = source.ContactType;
        Notes = source.Notes;
        UpdateAuditInfo(source.Audit);
        return this;
    }

    public ConsumerContact Map()
    {
        return new ConsumerContact
        {
            ContactId = Id,
            ConsumerId = ConsumerId,
            TenantId = TenantId,
            ContactName = ContactName,
            ContactEmail = ContactEmail,
            ContactPhone = ContactPhone,
            ContactAddress = ContactAddress,
            CompanyDivision = CompanyDivision,
            ContactDesignation = ContactDesignation,
            IsPrimaryContact = IsPrimaryContact,
            ContactType = ContactType,
            Notes = Notes,
            Audit = MapAuditInfo()
        };
    }

    // Legacy methods for compatibility (keeping the old methods)
    public ConsumerContact ToModel()
    {
        return Map();
    }

    public static ConsumerContact ToModel(ConsumerContactEntity entity)
    {
        return entity.Map();
    }
}
