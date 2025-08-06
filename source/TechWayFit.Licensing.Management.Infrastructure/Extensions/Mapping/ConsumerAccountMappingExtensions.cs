using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.Common;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between ConsumerAccount core model and ConsumerAccountEntity
/// </summary>
public static class ConsumerAccountMappingExtensions
{
    /// <summary>
    /// Converts ConsumerAccount core model to ConsumerAccountEntity
    /// </summary>
    public static ConsumerAccountEntity ToEntity(this ConsumerAccount model)
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
    public static ConsumerAccount ToModel(this ConsumerAccountEntity entity)
    {
        if (entity == null) return null!;

        return new ConsumerAccount
        {
            ConsumerId = entity.Id,
            CompanyName = entity.CompanyName,
            AccountCode = entity.AccountCode,
            PrimaryContact = new ContactPerson
            {
                Name = entity.PrimaryContactName,
                Email = entity.PrimaryContactEmail,
                Phone = entity.PrimaryContactPhone,
                Position = entity.PrimaryContactPosition ?? string.Empty
            },
            SecondaryContact = new ContactPerson
            {
                Name = entity.SecondaryContactName ?? string.Empty,
                Email = entity.SecondaryContactEmail ?? string.Empty,
                Phone = entity.SecondaryContactPhone ?? string.Empty,
                Position = entity.SecondaryContactPosition ?? string.Empty
            },
            ActivatedAt = entity.ActivatedAt,
            SubscriptionEnd = entity.SubscriptionEnd,
            Audit = new AuditInfo
            {
                IsActive = entity.IsActive,
                IsDeleted = entity.IsDeleted,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedBy = entity.UpdatedBy,
                UpdatedOn = entity.UpdatedOn,
                DeletedBy = entity.DeletedBy,
                DeletedOn = entity.DeletedOn,
                RowVersion = entity.RowVersion
            },
            Address = new Address
            {
                Street = entity.AddressStreet,
                City = entity.AddressCity,
                State = entity.AddressState,
                PostalCode = entity.AddressPostalCode,
                Country = entity.AddressCountry
            },
            Notes = entity.Notes,
            Status = Enum.TryParse<ConsumerStatus>(entity.Status, out var status) ? status : ConsumerStatus.Prospect
        };
    }

    /// <summary>
    /// Updates existing ConsumerAccountEntity with values from ConsumerAccount core model
    /// </summary>
    public static void UpdateFromModel(this ConsumerAccountEntity entity, ConsumerAccount model)
    {
        if (entity == null || model == null) return;

        entity.CompanyName = model.CompanyName;
        entity.AccountCode = model.AccountCode;
        entity.PrimaryContactName = model.PrimaryContact.Name;
        entity.PrimaryContactEmail = model.PrimaryContact.Email;
        entity.PrimaryContactPhone = model.PrimaryContact.Phone;
        entity.PrimaryContactPosition = model.PrimaryContact.Position;
        entity.SecondaryContactName = model.SecondaryContact?.Name;
        entity.SecondaryContactEmail = model.SecondaryContact?.Email;
        entity.SecondaryContactPhone = model.SecondaryContact?.Phone;
        entity.SecondaryContactPosition = model.SecondaryContact?.Position;
        entity.ActivatedAt = model.ActivatedAt;
        entity.SubscriptionEnd = model.SubscriptionEnd;
        entity.IsActive = model.Audit.IsActive;
        entity.IsDeleted = model.Audit.IsDeleted;
        entity.AddressStreet = model.Address.Street;
        entity.AddressCity = model.Address.City;
        entity.AddressState = model.Address.State;
        entity.AddressPostalCode = model.Address.PostalCode;
        entity.AddressCountry = model.Address.Country;
        entity.Notes = model.Notes;
        entity.Status = model.Status.ToString();
        entity.UpdatedBy = model.Audit.UpdatedBy;
        entity.UpdatedOn = model.Audit.UpdatedOn;
        entity.DeletedBy = model.Audit.DeletedBy;
        entity.DeletedOn = model.Audit.DeletedOn;
        entity.RowVersion = model.Audit.RowVersion;
    }
}
