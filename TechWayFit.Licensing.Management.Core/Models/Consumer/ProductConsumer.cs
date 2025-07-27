using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Models.Consumer;

public class ProductConsumer
{
    public string ProductConsumerId { get; set; } = Guid.NewGuid().ToString();
    public ConsumerAccount Consumer { get; set; } = new();
    public EnterpriseProduct Product { get; set; } = new();
    public ContactPerson AccountManager { get; set; } = new();
}
