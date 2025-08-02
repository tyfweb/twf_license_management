using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.User;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Configuration;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Audit;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Consumer;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.License;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Notification;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Product;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.Settings;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Repositories.User;

namespace TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Data;

/// <summary>
/// PostgreSQL implementation of Unit of Work pattern
/// </summary>
public class PostgreSqlUnitOfWork : IUnitOfWork
{
    private readonly PostgreSqlPostgreSqlLicensingDbContext _context;
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

    public PostgreSqlUnitOfWork(PostgreSqlPostgreSqlLicensingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Core Repositories

    public IProductRepository Products => 
        _products ??= new PostgreSqlProductRepository(_context);

    public IProductLicenseRepository Licenses => 
        _licenses ??= new PostgreSqlProductLicenseRepository(_context);

    public IConsumerAccountRepository Consumers => 
        _consumers ??= new PostgreSqlConsumerAccountRepository(_context);

    public IUserProfileRepository Users => 
        _users ??= new PostgreSqlUserProfileRepository(_context);

    #endregion

    #region Supporting Repositories

    public IProductFeatureRepository ProductFeatures => 
        _productFeatures ??= new PostgreSqlProductFeatureRepository(_context);

    public IProductTierRepository ProductTiers => 
        _productTiers ??= new PostgreSqlProductTierRepository(_context);

    public IProductVersionRepository ProductVersions => 
        _productVersions ??= new PostgreSqlProductVersionRepository(_context);

    public IAuditEntryRepository AuditEntries => 
        _auditEntries ??= new PostgreSqlAuditEntryRepository(_context);

    public INotificationTemplateRepository NotificationTemplates => 
        _notificationTemplates ??= new PostgreSqlNotificationTemplateRepository(_context);

    public INotificationHistoryRepository NotificationHistory => 
        _notificationHistory ??= new PostgreSqlNotificationHistoryRepository(_context);

    public ISettingRepository Settings => 
        _settings ??= new PostgreSqlSettingRepository(_context);

    public IUserRoleRepository UserRoles => 
        _userRoles ??= new PostgreSqlUserRoleRepository(_context);

    public IUserRoleMappingRepository UserRoleMappings => 
        _userRoleMappings ??= new PostgreSqlUserRoleMappingRepository(_context);

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
