using System;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
[Table("product_consumers")]
public class ProductConsumerEntity : BaseAuditEntity
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

    public static ProductConsumerEntity FromModel(ProductConsumer model)
    {
        return new ProductConsumerEntity
        {
            Id = model.ProductConsumerId,
            ProductId = model.Product.ProductId,
            ConsumerId = model.Consumer.ConsumerId,
            AccountManagerName = model.AccountManager.Name,
            AccountManagerEmail = model.AccountManager.Email,
            AccountManagerPhone = model.AccountManager.Phone,
            AccountManagerPosition = model.AccountManager.Position,
            CreatedBy = "system", // Assuming system created this entry
            CreatedOn = DateTime.UtcNow,
            UpdatedBy = "system", // Assuming system updated this entry
            UpdatedOn = DateTime.UtcNow,
        
        };
    }
    public ProductConsumer ToModel()
    {
        return new ProductConsumer
        {
            ProductConsumerId = Id,
            Consumer = Consumer.ToModel(),
            Product = Product.ToModel(),
            AccountManager = new ContactPerson
            {
                Name = AccountManagerName,
                Email = AccountManagerEmail,
                Phone = AccountManagerPhone,
                Position = AccountManagerPosition
            }
        };
    }

}
