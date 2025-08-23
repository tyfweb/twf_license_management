using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Seeding;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Tenants;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

/// <summary>
/// Unit of Work pattern interface for coordinating repository operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    #region Core Repositories

    /// <summary>
    /// Product repository
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Product License repository  
    /// </summary>
    IProductLicenseRepository Licenses { get; }

    /// <summary>
    /// Product Activation repository
    /// </summary>
    IProductActivationRepository ProductActivations { get; }

    /// <summary>
    /// Consumer Account repository
    /// </summary>
    IConsumerAccountRepository Consumers { get; }

    /// <summary>
    /// Consumer Contact repository
    /// </summary>
    IConsumerContactRepository ConsumerContacts { get; }

    /// <summary>
    /// User Profile repository
    /// </summary>
    IUserProfileRepository Users { get; }

    #endregion

    #region Supporting Repositories

    /// <summary>
    /// Product Keys repository
    /// </summary>
    IProductKeysRepository ProductKeys { get; }

    /// <summary>
    /// Product Feature repository
    /// </summary>
    IProductFeatureRepository ProductFeatures { get; }

    /// <summary>
    /// Product Tier repository
    /// </summary>
    IProductTierRepository ProductTiers { get; }

    /// <summary>
    /// Product Feature Tier Mapping repository
    /// </summary>
    IProductFeatureTierMappingRepository ProductFeatureTierMappings { get; }

    /// <summary>
    /// Product Version repository
    /// </summary>
    IProductVersionRepository ProductVersions { get; }

    /// <summary>
    /// Audit Entry repository
    /// </summary>
    IAuditEntryRepository AuditEntries { get; }

    /// <summary>
    /// Notification Template repository
    /// </summary>
    INotificationTemplateRepository NotificationTemplates { get; }

    /// <summary>
    /// Notification History repository
    /// </summary>
    INotificationHistoryRepository NotificationHistory { get; }

    /// <summary>
    /// Settings repository
    /// </summary>
    ISettingRepository Settings { get; }

    /// <summary>
    /// User Role repository
    /// </summary>
    IUserRoleRepository UserRoles { get; }    /// <summary>
    /// User Role Mapping repository
    /// </summary>
    IUserRoleMappingRepository UserRoleMappings { get; }

    /// <summary>
    /// Role Permission repository
    /// </summary>
    IRolePermissionRepository RolePermissions { get; }

    #endregion

    #region Workflow Repositories

    /// <summary>
    /// Workflow History repository
    /// </summary>
    IWorkflowHistoryRepository WorkflowHistory { get; }

    /// <summary>
    /// Consumer Account Approval repository
    /// </summary>
    IApprovalRepository<Core.Models.Consumer.ConsumerAccount> ConsumerAccountApprovals { get; }

    /// <summary>
    /// Enterprise Product Approval repository
    /// </summary>
    IApprovalRepository<Core.Models.Product.EnterpriseProduct> EnterpriseProductApprovals { get; }

    /// <summary>
    /// Product License Approval repository
    /// </summary>
    IApprovalRepository<Core.Models.License.ProductLicense> ProductLicenseApprovals { get; }

    /// <summary>
    /// Seeding History repository
    /// </summary>
    ISeedingHistoryRepository SeedingHistory { get; }

    #endregion
    /// <summary>
    /// Tenant repository
    /// </summary>
    ITenantRepository Tenants { get; }

    #region Transaction Management

    /// <summary>
    /// Saves all changes made within the unit of work
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    #endregion


}
