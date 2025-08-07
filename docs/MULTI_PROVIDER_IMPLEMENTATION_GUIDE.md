# Multi-Provider Data Access Implementation Guide

## Overview

This document outlines how to extend the current PostgreSQL infrastructure to support multiple data providers: **EF InMemory**, **EF SQL Server**, and **File-based repositories**.

## Current Architecture Analysis

### ✅ Excellent Foundation for Multi-Provider Support

Your architecture is **already well-designed** for this scenario:

```
Infrastructure.Contracts (Interfaces)
    ↓
Infrastructure.PostgreSQL (Current Implementation)
Infrastructure.SqlServer (New - Easy)
Infrastructure.InMemory (New - Easy) 
Infrastructure.FileSystem (New - Moderate)
```

## Implementation Strategy

### 1. Provider-Agnostic Interface Design ✅ (Already Done)

Your current interfaces are provider-neutral:

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ILicenseRepository Licenses { get; }
    // ... other repositories
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### 2. Provider-Specific Implementations

## Project Structure

```
TechWayFit.Licensing.Management.Infrastructure.SqlServer/
├── Data/
│   ├── SqlServerLicensingDbContext.cs
│   └── SqlServerUnitOfWork.cs
├── Extensions/
│   └── SqlServerServiceExtensions.cs
├── Repositories/
│   ├── SqlServerLicenseRepository.cs
│   └── SqlServerProductRepository.cs
└── Configuration/
    └── SqlServerDatabaseConfiguration.cs

TechWayFit.Licensing.Management.Infrastructure.InMemory/
├── Data/
│   ├── InMemoryLicensingDbContext.cs
│   └── InMemoryUnitOfWork.cs
├── Extensions/
│   └── InMemoryServiceExtensions.cs
└── Repositories/
    ├── InMemoryLicenseRepository.cs
    └── InMemoryProductRepository.cs

TechWayFit.Licensing.Management.Infrastructure.FileSystem/
├── Data/
│   ├── FileSystemUnitOfWork.cs
│   └── FileSystemStorage.cs
├── Extensions/
│   └── FileSystemServiceExtensions.cs
├── Repositories/
│   ├── FileBasedLicenseRepository.cs
│   └── FileBasedProductRepository.cs
└── Serialization/
    ├── IFileSerializer.cs
    └── JsonFileSerializer.cs
```

---

## Implementation Examples

### 1. SQL Server Provider (EASY - 2-3 hours)

#### SqlServerServiceExtensions.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Extensions;

public static class SqlServerServiceExtensions
{
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("SQL Server connection string is not configured.");
        }

        // Add DbContext with SQL Server provider
        services.AddDbContext<SqlServerLicensingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                sqlOptions.CommandTimeout(60);
            })
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, SqlServerUnitOfWork>();

        return services;
    }
}
```

#### SqlServerLicensingDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using TechWayFit.Licensing.Management.Infrastructure.PostgreSql.Models.Entities.Common;

namespace TechWayFit.Licensing.Management.Infrastructure.SqlServer.Data;

public class SqlServerLicensingDbContext : DbContext
{
    public SqlServerLicensingDbContext(DbContextOptions<SqlServerLicensingDbContext> options) 
        : base(options)
    {
    }

    // Same DbSets as PostgreSQL context
    public DbSet<LicenseEntity> Licenses => Set<LicenseEntity>();
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    // ... other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Same model configuration as PostgreSQL
        // But with SQL Server-specific configurations
        
        modelBuilder.Entity<LicenseEntity>(entity =>
        {
            entity.ToTable("Licenses", "dbo"); // SQL Server schema
            entity.HasKey(e => e.Id);
            // ... other configurations
        });

        // Apply multi-tenant global filters
        modelBuilder.Entity<BaseEntity>()
            .HasQueryFilter(e => e.TenantId == _userContext.TenantId);

        base.OnModelCreating(modelBuilder);
    }
}
```

### 2. InMemory Provider (VERY EASY - 1-2 hours)

#### InMemoryServiceExtensions.cs
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Extensions;

public static class InMemoryServiceExtensions
{
    public static IServiceCollection AddInMemoryInfrastructure(
        this IServiceCollection services,
        string databaseName = "LicensingInMemoryDb")
    {
        // Add DbContext with InMemory provider
        services.AddDbContext<InMemoryLicensingDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName)
                   .EnableServiceProviderCaching()
                   .EnableSensitiveDataLogging(true); // OK for testing
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

        return services;
    }
}
```

#### InMemoryLicensingDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;

namespace TechWayFit.Licensing.Management.Infrastructure.InMemory.Data;

public class InMemoryLicensingDbContext : DbContext
{
    public InMemoryLicensingDbContext(DbContextOptions<InMemoryLicensingDbContext> options) 
        : base(options)
    {
    }

    // Same DbSets and configurations as other providers
    // InMemory doesn't need special configurations
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Same model as PostgreSQL but without database-specific constraints
        // InMemory is more forgiving
        
        base.OnModelCreating(modelBuilder);
    }
}
```

### 3. File-Based Provider (MODERATE - 1-2 days)

#### FileSystemServiceExtensions.cs
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TechWayFit.Licensing.Management.Infrastructure.FileSystem.Extensions;

public static class FileSystemServiceExtensions
{
    public static IServiceCollection AddFileSystemInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.GetSection("FileSystem").Get<FileSystemConfiguration>()
                    ?? new FileSystemConfiguration();

        services.Configure<FileSystemConfiguration>(configuration.GetSection("FileSystem"));
        
        // Register file-based services
        services.AddSingleton<IFileSerializer, JsonFileSerializer>();
        services.AddSingleton<IFileSystemStorage, FileSystemStorage>();
        
        // Register repositories
        services.AddScoped<ILicenseRepository, FileBasedLicenseRepository>();
        services.AddScoped<IProductRepository, FileBasedProductRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, FileSystemUnitOfWork>();

        return services;
    }
}
```

#### FileBasedLicenseRepository.cs
```csharp
using Microsoft.Extensions.Options;
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Repositories.License;

namespace TechWayFit.Licensing.Management.Infrastructure.FileSystem.Repositories;

public class FileBasedLicenseRepository : ILicenseRepository
{
    private readonly IFileSystemStorage _storage;
    private readonly IFileSerializer _serializer;
    private readonly FileSystemConfiguration _config;
    private readonly string _filePath;

    public FileBasedLicenseRepository(
        IFileSystemStorage storage,
        IFileSerializer serializer,
        IOptions<FileSystemConfiguration> config)
    {
        _storage = storage;
        _serializer = serializer;
        _config = config.Value;
        _filePath = Path.Combine(_config.DataPath, "licenses.json");
    }

    public async Task<LicenseModel> AddAsync(LicenseModel license, CancellationToken cancellationToken = default)
    {
        var licenses = await LoadAllLicensesAsync();
        
        license.Id = Guid.NewGuid();
        license.CreatedOn = DateTime.UtcNow;
        
        licenses.Add(license);
        
        await SaveAllLicensesAsync(licenses);
        
        return license;
    }

    public async Task<LicenseModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var licenses = await LoadAllLicensesAsync();
        return licenses.FirstOrDefault(l => l.Id == id);
    }

    public async Task<IEnumerable<LicenseModel>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var licenses = await LoadAllLicensesAsync();
        return licenses.Where(l => l.TenantId == tenantId);
    }

    private async Task<List<LicenseModel>> LoadAllLicensesAsync()
    {
        if (!await _storage.FileExistsAsync(_filePath))
        {
            return new List<LicenseModel>();
        }

        var json = await _storage.ReadAllTextAsync(_filePath);
        return _serializer.Deserialize<List<LicenseModel>>(json) ?? new List<LicenseModel>();
    }

    private async Task SaveAllLicensesAsync(List<LicenseModel> licenses)
    {
        var json = _serializer.Serialize(licenses);
        await _storage.WriteAllTextAsync(_filePath, json);
    }
}
```

#### FileSystemUnitOfWork.cs
```csharp
using TechWayFit.Licensing.Management.Infrastructure.Contracts.Data;

namespace TechWayFit.Licensing.Management.Infrastructure.FileSystem.Data;

public class FileSystemUnitOfWork : IUnitOfWork
{
    private readonly IFileSystemStorage _storage;
    private readonly Dictionary<string, object> _repositories;
    private readonly List<Func<Task>> _pendingOperations;

    public FileSystemUnitOfWork(
        ILicenseRepository licenseRepository,
        IProductRepository productRepository,
        // ... other repositories
        IFileSystemStorage storage)
    {
        Licenses = licenseRepository;
        Products = productRepository;
        _storage = storage;
        _repositories = new Dictionary<string, object>();
        _pendingOperations = new List<Func<Task>>();
    }

    public ILicenseRepository Licenses { get; }
    public IProductRepository Products { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Execute all pending operations in sequence
        var operationCount = _pendingOperations.Count;
        
        foreach (var operation in _pendingOperations)
        {
            await operation();
        }
        
        _pendingOperations.Clear();
        return operationCount;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // File-based operations are inherently transactional at the file level
        // For more complex scenarios, could implement backup/restore mechanism
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _pendingOperations.Clear();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _repositories.Clear();
        _pendingOperations.Clear();
    }
}
```

---

## Configuration Examples

### appsettings.json
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=licensing;Username=app;Password=secret;SSL Mode=Require",
    "SqlServer": "Server=localhost;Database=Licensing;Trusted_Connection=true;MultipleActiveResultSets=true",
    "InMemory": "LicensingTestDb"
  },
  "DataProvider": "PostgreSQL", // PostgreSQL | SqlServer | InMemory | FileSystem
  "FileSystem": {
    "DataPath": "./Data",
    "BackupPath": "./Backups",
    "EnableBackups": true,
    "BackupRetentionDays": 30
  }
}
```

### Provider Selection in Program.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
    var dataProvider = Configuration["DataProvider"] ?? "PostgreSQL";

    switch (dataProvider.ToUpperInvariant())
    {
        case "POSTGRESQL":
            services.AddPostgreSqlInfrastructure(Configuration);
            break;
            
        case "SQLSERVER":
            services.AddSqlServerInfrastructure(Configuration);
            break;
            
        case "INMEMORY":
            services.AddInMemoryInfrastructure();
            break;
            
        case "FILESYSTEM":
            services.AddFileSystemInfrastructure(Configuration);
            break;
            
        default:
            throw new InvalidOperationException($"Unsupported data provider: {dataProvider}");
    }
}
```

---

## Testing Strategy

### Multi-Provider Integration Tests
```csharp
[TestFixture]
public class MultiProviderIntegrationTests
{
    [Test]
    [TestCase("PostgreSQL")]
    [TestCase("SqlServer")] 
    [TestCase("InMemory")]
    [TestCase("FileSystem")]
    public async Task LicenseRepository_ShouldWork_WithAllProviders(string provider)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(provider);
        
        ConfigureDataProvider(services, configuration, provider);
        
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        // Act & Assert
        var license = new LicenseModel 
        { 
            Name = "Test License", 
            TenantId = Guid.NewGuid() 
        };
        
        var savedLicense = await unitOfWork.Licenses.AddAsync(license);
        await unitOfWork.SaveChangesAsync();
        
        var retrievedLicense = await unitOfWork.Licenses.GetByIdAsync(savedLicense.Id);
        
        Assert.That(retrievedLicense, Is.Not.Null);
        Assert.That(retrievedLicense.Name, Is.EqualTo("Test License"));
    }
}
```

---

## Implementation Effort Estimation

| Provider | Effort | Time | Complexity | Notes |
|----------|--------|------|------------|-------|
| **SQL Server** | Easy | 2-3 hours | Low | Copy PostgreSQL, change provider |
| **InMemory** | Very Easy | 1-2 hours | Very Low | Perfect for testing |
| **File-based** | Moderate | 1-2 days | Medium | Custom serialization, transaction handling |

## Benefits of Multi-Provider Support

### ✅ **Advantages**
- **Flexibility**: Choose best provider for environment
- **Testing**: InMemory for unit tests, real DB for integration
- **Development**: File-based for rapid prototyping
- **Deployment**: Different providers for different environments
- **Migration**: Easy to switch between providers

### ⚠️ **Considerations**
- **Provider-specific features**: Some advanced features may not work across all providers
- **Performance characteristics**: Each provider has different performance profiles
- **Transaction handling**: File-based has limited transaction support
- **Concurrency**: File-based provider may have concurrency limitations

---

## Conclusion

Your current architecture makes multi-provider support **EASY to MODERATE**:

- **SQL Server**: Very easy (2-3 hours) - mostly configuration changes
- **InMemory**: Very easy (1-2 hours) - perfect for testing
- **File-based**: Moderate (1-2 days) - requires custom implementation

The **interface-based design** and **Unit of Work pattern** you've already implemented are perfect for this scenario. Your abstractions are provider-agnostic, making the implementation straightforward.

**Recommendation**: Start with **InMemory** for testing, then **SQL Server** for production alternatives, and **File-based** only if you need offline/portable scenarios.
