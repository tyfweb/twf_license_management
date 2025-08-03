using System.Linq.Expressions;
using TechWayFit.Licensing.Core.Models;
using TechWayFit.Licensing.Management.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Management.Infrastructure.Models.Entities.License;

public class LicenseSearchRequest:SearchRequest<ProductLicenseEntity>
{
    public string ProductId { get; set; }
    public string ConsumerId { get; set; }
    public LicenseStatus? Status { get; set; }
    public LicenseSearchRequest(string productId = "", string consumerId = "", LicenseStatus? status = null)
    {
        ProductId = productId;
        ConsumerId = consumerId;
        Status = status;
        Filters = new List<Expression<Func<ProductLicenseEntity, bool>>>();

        if (!string.IsNullOrEmpty(productId) && Guid.TryParse(productId, out var productGuid))
            Filters.Add(license => license.ProductId == productGuid);
        if (!string.IsNullOrEmpty(consumerId) && Guid.TryParse(consumerId, out var consumerGuid))
            Filters.Add(license => license.ConsumerId == consumerGuid);
        if (status.HasValue)
            Filters.Add(license => license.Status == status.Value.ToString());
    }
}
