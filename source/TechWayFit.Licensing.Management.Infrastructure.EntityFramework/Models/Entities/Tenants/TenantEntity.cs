using System;
using TechWayFit.Licensing.Management.Core.Models.Tenant;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Models.Entities.Tenants;

public class TenantEntity: AuditEntity, IEntityMapper<Tenant, TenantEntity>
{
    public string TenantCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    public TenantEntity Map(Tenant model)
    {
        Id = model.TenantId;
        TenantCode = model.TenantCode;
        Description = model.Description;
        TenantName = model.TenantName;
        return this;
    }
 

    public Tenant Map()
    {
        return new Tenant
        {
            TenantId = this.Id,
            TenantName = this.TenantName,
            TenantCode = this.TenantCode,
            Description = this.Description
        };
    }
}
