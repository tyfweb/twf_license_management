using TechWayFit.Licensing.Management.Core.Contracts.Services.Workflow; 
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
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
        IApprovalRepository<ConsumerAccount> repository,
        IWorkflowHistoryRepository historyRepository,
        IUserContext userContext,
        ILogger<WorkflowService<ConsumerAccount>> logger)
        : base(repository, historyRepository, userContext, logger)
    {
    }
}

/// <summary>
/// Workflow service implementation for EnterpriseProduct
/// </summary>
public class EnterpriseProductWorkflowService : WorkflowService<EnterpriseProduct>, IEnterpriseProductWorkflowService
{
    public EnterpriseProductWorkflowService(
        IApprovalRepository<EnterpriseProduct> repository,
        IWorkflowHistoryRepository historyRepository,
        IUserContext userContext,
        ILogger<WorkflowService<EnterpriseProduct>> logger)
        : base(repository, historyRepository, userContext, logger)
    {
    }
}

/// <summary>
/// Workflow service implementation for ProductLicense
/// </summary>
public class ProductLicenseWorkflowService : WorkflowService<ProductLicense>, IProductLicenseWorkflowService
{
    public ProductLicenseWorkflowService(
        IApprovalRepository<ProductLicense> repository,
        IWorkflowHistoryRepository historyRepository,
        IUserContext userContext,
        ILogger<WorkflowService<ProductLicense>> logger)
        : base(repository, historyRepository, userContext, logger)
    {
    }
}
