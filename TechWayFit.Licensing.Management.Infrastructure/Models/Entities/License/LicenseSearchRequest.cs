using TechWayFit.Licensing.Infrastructure.Models.Search;
using TechWayFit.Licensing.Management.Core.Models.License;

namespace TechWayFit.Licensing.Infrastructure.Models.Entities.License;

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
        Filters = new List<Func<ProductLicenseEntity, bool>>();
        if (!string.IsNullOrEmpty(productId))
            Filters.Add(license => license.ProductId == productId);
        if (!string.IsNullOrEmpty(consumerId))
            Filters.Add(license => license.ConsumerId == consumerId);
        if (status.HasValue)
            Filters.Add(license => license.Status == status.Value.ToString());
    }
}
