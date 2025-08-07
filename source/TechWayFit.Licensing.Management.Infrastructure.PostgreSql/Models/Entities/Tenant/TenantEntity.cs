using System;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Tenant;

public class TenantEntity: AuditEntity
{
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public string TenantName { get; set; } = string.Empty;

}
