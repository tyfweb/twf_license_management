using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping between ProductConsumer core model and ProductConsumerEntity
/// </summary>
public static class ProductConsumerMappingExtensions
{
    /// <summary>
    /// Converts ProductConsumer core model to ProductConsumerEntity
    /// </summary>
    public static ProductConsumerEntity ToEntity(this ProductConsumer model)
    {
        if (model == null) return null!;

        return new ProductConsumerEntity
        {
            Id = model.ProductConsumerId,
            ProductId = model.Product?.ProductId ?? Guid.Empty,
            ConsumerId = model.Consumer?.ConsumerId ?? Guid.Empty,
            AccountManagerName = model.AccountManager?.Name ?? string.Empty,
            AccountManagerEmail = model.AccountManager?.Email ?? string.Empty,
            AccountManagerPhone = model.AccountManager?.Phone ?? string.Empty,
            AccountManagerPosition = model.AccountManager?.Position ?? string.Empty,
            IsActive = true,
            CreatedBy = "System",
            CreatedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Converts ProductConsumerEntity to ProductConsumer core model
    /// </summary>
    public static ProductConsumer ToModel(this ProductConsumerEntity entity)
    {
        if (entity == null) return null!;

        return new ProductConsumer
        {
            ProductConsumerId = entity.Id,
            Consumer = entity.Consumer?.ToModel() ?? new ConsumerAccount(),
            Product = entity.Product?.ToModel() ?? new Core.Models.Product.EnterpriseProduct(),
            AccountManager = new ContactPerson
            {
                Name = entity.AccountManagerName,
                Email = entity.AccountManagerEmail,
                Phone = entity.AccountManagerPhone,
                Position = entity.AccountManagerPosition
            }
        };
    }

    /// <summary>
    /// Updates existing ProductConsumerEntity with values from ProductConsumer core model
    /// </summary>
    public static void UpdateFromModel(this ProductConsumerEntity entity, ProductConsumer model)
    {
        if (entity == null || model == null) return;

        entity.ProductId = model.Product?.ProductId ?? entity.ProductId;
        entity.ConsumerId = model.Consumer?.ConsumerId ?? entity.ConsumerId;
        entity.AccountManagerName = model.AccountManager?.Name ?? entity.AccountManagerName;
        entity.AccountManagerEmail = model.AccountManager?.Email ?? entity.AccountManagerEmail;
        entity.AccountManagerPhone = model.AccountManager?.Phone ?? entity.AccountManagerPhone;
        entity.AccountManagerPosition = model.AccountManager?.Position ?? entity.AccountManagerPosition;
        entity.UpdatedBy = "System";
        entity.UpdatedOn = DateTime.UtcNow;
    }
}
