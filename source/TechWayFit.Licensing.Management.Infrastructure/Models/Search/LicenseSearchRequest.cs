using System.Linq.Expressions;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search; 
namespace TechWayFit.Licensing.Management.Infrastructure.Models.Search;
public class LicenseSearchRequest:SearchRequest<License>
{
    public string ProductId { get; set; }
    public string ConsumerId { get; set; }
    public LicenseStatus? Status { get; set; }
    public LicenseSearchRequest(string productId = "", string consumerId = "", LicenseStatus? status = null)
    {
        ProductId = productId;
        ConsumerId = consumerId;
        Status = status;
        Filters = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(productId) && Guid.TryParse(productId, out var productGuid))
            Filters.Add(nameof(ProductId), productGuid.ToString());
        if (!string.IsNullOrEmpty(consumerId) && Guid.TryParse(consumerId, out var consumerGuid))
            Filters.Add(nameof(ConsumerId), consumerGuid.ToString());
        if (status.HasValue)
            Filters.Add(nameof(Status), status.Value.ToString());
    }
}
