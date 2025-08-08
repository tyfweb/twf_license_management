using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechWayFit.Licensing.Management.Core.Contracts;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Workflow;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.EntityFramework.Repositories.Workflow;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Data;

/// <summary>
/// InMemory implementation of Unit of Work pattern
/// </summary>
public class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly InMemoryLicensingDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Core repositories
    private IProductRepository? _products;
    private IProductLicenseRepository? _licenses;
    private IConsumerAccountRepository? _consumers;
    private IUserProfileRepository? _users;

    // Supporting repositories
    private IProductFeatureRepository? _productFeatures;
    private IProductTierRepository? _productTiers;
    private IProductVersionRepository? _productVersions;
    private IAuditEntryRepository? _auditEntries;
    private INotificationTemplateRepository? _notificationTemplates;
    private INotificationHistoryRepository? _notificationHistory;
    private ISettingRepository? _settings;
    private IUserRoleRepository? _userRoles;
    private IUserRoleMappingRepository? _userRoleMappings;
    
    // Workflow repositories
    private IWorkflowHistoryRepository? _workflowHistory;
    private IApprovalRepository<Core.Models.Consumer.ConsumerAccount>? _consumerAccountApprovals;
    
    private readonly IUserContext _userContext;

    public InMemoryUnitOfWork(InMemoryLicensingDbContext context, IUserContext userContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    #region Core Repositories

    public IProductRepository Products => 
        _products ??= new EfCoreProductRepository(_context, _userContext);

    public IProductLicenseRepository Licenses => 
        _licenses ??= new EfCoreProductLicenseRepository(_context, _userContext);

    public IConsumerAccountRepository Consumers => 
        _consumers ??= new EfCoreConsumerAccountRepository(_context, _userContext);

    public IUserProfileRepository Users => 
        _users ??= new EfCoreUserProfileRepository(_context, _userContext);

    #endregion

    #region Supporting Repositories

    public IProductFeatureRepository ProductFeatures => 
        _productFeatures ??= new EfCoreProductFeatureRepository(_context, _userContext);

    public IProductTierRepository ProductTiers => 
        _productTiers ??= new EfCoreProductTierRepository(_context, _userContext);

    public IProductVersionRepository ProductVersions => 
        _productVersions ??= new EfCoreProductVersionRepository(_context, _userContext);

    public IAuditEntryRepository AuditEntries => 
        _auditEntries ??= new EfCoreAuditEntryRepository(_context, _userContext);

    public INotificationTemplateRepository NotificationTemplates => 
        _notificationTemplates ??= new EfCoreNotificationTemplateRepository(_context, _userContext);

    public INotificationHistoryRepository NotificationHistory => 
        _notificationHistory ??= new EfCoreNotificationHistoryRepository(_context, _userContext);

    public ISettingRepository Settings => 
        _settings ??= new EfCoreSettingRepository(_context, _userContext);

    public IUserRoleRepository UserRoles => 
        _userRoles ??= new EfCoreUserRoleRepository(_context, _userContext);

    public IUserRoleMappingRepository UserRoleMappings => 
        _userRoleMappings ??= new EfCoreUserRoleMappingRepository(_context, _userContext);

    #endregion

    #region Workflow Repositories

    public IWorkflowHistoryRepository WorkflowHistory => 
        _workflowHistory ??= new EfCoreWorkflowHistoryRepository(_context, _userContext);

    public IApprovalRepository<Core.Models.Consumer.ConsumerAccount> ConsumerAccountApprovals => 
        _consumerAccountApprovals ??= new EfCoreConsumerAccountApprovalRepository(_context, _userContext);

    #endregion

    #region Transaction Management

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region Dispose Pattern

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }

    #endregion
}
