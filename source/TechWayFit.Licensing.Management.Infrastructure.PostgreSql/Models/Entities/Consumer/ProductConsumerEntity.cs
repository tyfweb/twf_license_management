using System;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Consumer;

[Table("product_consumers")]
public class ProductConsumerEntity : AuditEntity, IEntityMapper<ProductConsumer, ProductConsumerEntity>
{

    /// <summary>
    /// Identifier for the associated product
    /// </summary>
    public Guid ProductId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier for the associated consumer
    /// </summary>
    public Guid ConsumerId { get; set; } = Guid.NewGuid();

    public string AccountManagerName { get; set; } = string.Empty;
    public string AccountManagerEmail { get; set; } = string.Empty;
    public string AccountManagerPhone { get; set; } = string.Empty;
    public string AccountManagerPosition { get; set; } = string.Empty;
    public ConsumerAccountEntity Consumer { get; set; } = new();
    public ProductEntity Product { get; set; } = new();

    #region IEntityMapper Implementation
    public ProductConsumerEntity Map(ProductConsumer model)
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
    public ProductConsumer Map()
    {
        return new ProductConsumer
        {
            ProductConsumerId = this.Id,
            Consumer = this.Consumer?.Map() ?? new ConsumerAccount(),
            Product = this.Product?.Map() ?? new Core.Models.Product.EnterpriseProduct(),
            AccountManager = new ContactPerson
            {
                Name = this.AccountManagerName,
                Email = this.AccountManagerEmail,
                Phone = this.AccountManagerPhone,
                Position = this.AccountManagerPosition
            }
        };
    }
    #endregion
}
