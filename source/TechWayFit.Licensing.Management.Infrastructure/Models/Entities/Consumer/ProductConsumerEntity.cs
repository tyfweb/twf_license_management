using System;
using System.ComponentModel.DataAnnotations.Schema;
using TechWayFit.Licensing.Management.Infrastructure.Data.Entities.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Products;
using TechWayFit.Licensing.Management.Core.Models.Consumer;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.Consumer;
[Table("product_consumers")]
public class ProductConsumerEntity : AuditEntity
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
}
