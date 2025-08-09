using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow; 
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using Microsoft.Extensions.Logging;
using TechWayFit.Licensing.Management.Core.Models.Product;
using TechWayFit.Licensing.Management.Core.Models.Consumer;
using TechWayFit.Licensing.Management.Core.Models.License;
using TechWayFit.Licensing.Management.Core.Contracts;

namespace TechWayFit.Licensing.Management.Services.Implementations.Workflow;

/// <summary>
/// Workflow service implementation for ConsumerAccount
/// </summary>
public class ConsumerAccountWorkflowService : WorkflowService<ConsumerAccount>, IConsumerAccountWorkflowService
{
    public ConsumerAccountWorkflowService(
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<WorkflowService<ConsumerAccount>> logger)
        : base(unitOfWork.ConsumerAccountApprovals, unitOfWork.WorkflowHistory, userContext, logger)
    {
    }
}

/// <summary>
/// Workflow service implementation for EnterpriseProduct
/// </summary>
public class EnterpriseProductWorkflowService : WorkflowService<EnterpriseProduct>, IEnterpriseProductWorkflowService
{
    public EnterpriseProductWorkflowService(
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<WorkflowService<EnterpriseProduct>> logger)
        : base(unitOfWork.EnterpriseProductApprovals, unitOfWork.WorkflowHistory, userContext, logger)
    {
    }
}

/// <summary>
/// Workflow service implementation for ProductLicense
/// </summary>
public class ProductLicenseWorkflowService : WorkflowService<ProductLicense>, IProductLicenseWorkflowService
{
    public ProductLicenseWorkflowService(
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<WorkflowService<ProductLicense>> logger)
        : base(unitOfWork.ProductLicenseApprovals, unitOfWork.WorkflowHistory, userContext, logger)
    {
    }
}
