# Improved Multi-Provider Architecture Design

## Problem with Current Approach
- Duplicating repositories across PostgreSQL, InMemory, SQL Server
- Duplicating entity configurations
- Duplicating Unit of Work implementations
- Violates DRY principle

## Better Solution: Provider-Agnostic EF Core

### New Project Structure

```
TechWayFit.Licensing.Management.Infrastructure.EntityFramework/
├── Data/
│   ├── LicensingDbContext.cs (Abstract base)
│   └── EfCoreUnitOfWork.cs (Generic implementation)
├── Repositories/
│   ├── EfCoreBaseRepository.cs
│   ├── Product/
│   │   └── EfCoreProductRepository.cs
│   ├── License/
│   │   └── EfCoreLicenseRepository.cs
│   └── ...
├── Configuration/
│   ├── EntityConfigurations/
│   │   ├── ProductEntityConfiguration.cs
│   │   ├── LicenseEntityConfiguration.cs
│   │   └── ...
├── Extensions/
│   └── EfCoreServiceExtensions.cs (Base)
└── Models/
    └── (Shared entity models)

TechWayFit.Licensing.Management.Infrastructure.PostgreSQL/
├── Data/
│   └── PostgreSqlLicensingDbContext.cs (Inherits from base)
├── Extensions/
│   └── PostgreSqlServiceExtensions.cs
└── Configuration/
    └── PostgreSqlSpecificConfigurations.cs (Only PG-specific)

TechWayFit.Licensing.Management.Infrastructure.InMemory/
├── Data/
│   └── InMemoryLicensingDbContext.cs (Inherits from base)
├── Extensions/
│   └── InMemoryServiceExtensions.cs
└── Configuration/
    └── InMemorySpecificConfigurations.cs (Only testing-specific)

TechWayFit.Licensing.Management.Infrastructure.SqlServer/
├── Data/
│   └── SqlServerLicensingDbContext.cs (Inherits from base)
├── Extensions/
│   └── SqlServerServiceExtensions.cs
└── Configuration/
    └── SqlServerSpecificConfigurations.cs (Only SQL Server-specific)
```

## Implementation Strategy

### 1. Provider-Agnostic EF Core Project

#### Base DbContext (Abstract)
```csharp
public abstract class LicensingDbContext : DbContext
{
    protected readonly IUserContext _userContext;

    protected LicensingDbContext(DbContextOptions options, IUserContext userContext) 
        : base(options)
    {
        _userContext = userContext;
    }

    // Common DbSets (same across all providers)
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<LicenseEntity> Licenses { get; set; }
    // ... all other entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply common configurations
        ApplyCommonConfigurations(modelBuilder);
        
        // Apply provider-specific configurations (virtual method)
        ApplyProviderSpecificConfigurations(modelBuilder);
        
        // Apply multi-tenant filters
        ApplyTenantFilters(modelBuilder);
    }

    protected virtual void ApplyCommonConfigurations(ModelBuilder modelBuilder)
    {
        // Entity configurations that work across all providers
        modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());
        modelBuilder.ApplyConfiguration(new LicenseEntityConfiguration());
        // ... apply all common configurations
    }

    protected virtual void ApplyProviderSpecificConfigurations(ModelBuilder modelBuilder)
    {
        // Override in provider-specific contexts for provider-specific configs
    }

    // Common audit and tenant methods
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

#### Generic Repository Implementation
```csharp
public class EfCoreBaseRepository<TEntity> : IBaseRepository<TEntity> 
    where TEntity : BaseEntity, new()
{
    protected readonly LicensingDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly IUserContext _userContext;

    public EfCoreBaseRepository(LicensingDbContext context, IUserContext userContext)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _userContext = userContext;
    }

    // All repository methods implemented once
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid();
        entity.TenantId = _userContext.TenantId;
        // ... common logic
        
        _dbSet.Add(entity);
        return entity;
    }

    // ... all other repository methods
}
```

#### Generic Unit of Work
```csharp
public class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly LicensingDbContext _context;
    private readonly IUserContext _userContext;
    
    // Lazy-loaded repositories
    private IProductRepository? _products;
    private ILicenseRepository? _licenses;
    // ...

    public EfCoreUnitOfWork(LicensingDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public IProductRepository Products => 
        _products ??= new EfCoreProductRepository(_context, _userContext);

    public ILicenseRepository Licenses => 
        _licenses ??= new EfCoreLicenseRepository(_context, _userContext);

    // ... other repositories

    // Transaction methods implemented once
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ... other UoW methods
}
```

### 2. Provider-Specific Implementations

#### PostgreSQL Implementation
```csharp
public class PostgreSqlLicensingDbContext : LicensingDbContext
{
    public PostgreSqlLicensingDbContext(
        DbContextOptions<PostgreSqlLicensingDbContext> options, 
        IUserContext userContext) : base(options, userContext)
    {
    }

    protected override void ApplyProviderSpecificConfigurations(ModelBuilder modelBuilder)
    {
        base.ApplyProviderSpecificConfigurations(modelBuilder);

        // Only PostgreSQL-specific configurations
        modelBuilder.Entity<ProductEntity>()
            .ToTable("products", "licensing") // PostgreSQL schema
            .HasIndex(p => p.Name)
            .HasMethod("gin"); // PostgreSQL GIN index

        // PostgreSQL-specific features
        modelBuilder.Entity<AuditEntity>()
            .Property(a => a.Changes)
            .HasColumnType("jsonb"); // PostgreSQL JSONB
    }
}

// PostgreSQL Service Extension
public static class PostgreSqlServiceExtensions
{
    public static IServiceCollection AddPostgreSqlInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<LicensingDbContext, PostgreSqlLicensingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
                npgsqlOptions.CommandTimeout(60);
            });
        });

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
        
        return services;
    }
}
```

#### InMemory Implementation
```csharp
public class InMemoryLicensingDbContext : LicensingDbContext
{
    public InMemoryLicensingDbContext(
        DbContextOptions<InMemoryLicensingDbContext> options, 
        IUserContext userContext) : base(options, userContext)
    {
    }

    protected override void ApplyProviderSpecificConfigurations(ModelBuilder modelBuilder)
    {
        base.ApplyProviderSpecificConfigurations(modelBuilder);

        // InMemory-specific optimizations
        // Remove constraints that don't work well with InMemory
        modelBuilder.Entity<ProductEntity>()
            .Ignore(p => p.ComplexConstraintProperty);

        // Simplify for testing
        modelBuilder.Entity<AuditEntity>()
            .Property(a => a.Changes)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null));
    }
}

// InMemory Service Extension
public static class InMemoryServiceExtensions
{
    public static IServiceCollection AddInMemoryInfrastructure(
        this IServiceCollection services,
        string databaseName = "LicensingTestDb")
    {
        services.AddDbContext<LicensingDbContext, InMemoryLicensingDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName)
                   .EnableSensitiveDataLogging(true);
        });

        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        return services;
    }
}
```

#### SQL Server Implementation
```csharp
public class SqlServerLicensingDbContext : LicensingDbContext
{
    public SqlServerLicensingDbContext(
        DbContextOptions<SqlServerLicensingDbContext> options, 
        IUserContext userContext) : base(options, userContext)
    {
    }

    protected override void ApplyProviderSpecificConfigurations(ModelBuilder modelBuilder)
    {
        base.ApplyProviderSpecificConfigurations(modelBuilder);

        // SQL Server-specific configurations
        modelBuilder.Entity<ProductEntity>()
            .ToTable("Products", "dbo")
            .HasIndex(p => p.Name)
            .IsClustered(false); // SQL Server non-clustered index

        // SQL Server-specific features
        modelBuilder.Entity<AuditEntity>()
            .Property(a => a.Changes)
            .HasColumnType("nvarchar(max)"); // SQL Server NVARCHAR(MAX)

        // SQL Server temporal tables (if needed)
        modelBuilder.Entity<ProductEntity>()
            .ToTable(tb => tb.IsTemporal());
    }
}
```

### 3. Configuration in Program.cs

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
            
        default:
            throw new InvalidOperationException($"Unsupported data provider: {dataProvider}");
    }
}
```

## Benefits of This Approach

### ✅ **Advantages**
1. **DRY Principle**: Repository implementations written once
2. **Maintainability**: Common logic in one place
3. **Consistency**: Same behavior across all providers
4. **Flexibility**: Provider-specific optimizations where needed
5. **Testing**: Easy to switch between providers for testing

### 📊 **Code Reduction**
- **Repositories**: 1 implementation instead of 3-4
- **Unit of Work**: 1 implementation instead of 3-4
- **Entity Configurations**: Shared configurations + provider-specific only
- **Maintenance**: Single codebase for core functionality

### 🔧 **Provider-Specific Customizations**
- **PostgreSQL**: JSONB, GIN indexes, schemas
- **SQL Server**: Temporal tables, nvarchar(max), clustered indexes
- **InMemory**: Simplified constraints, testing optimizations

## Migration Strategy

1. **Create EF Core base project**
2. **Move common entities and configurations**
3. **Implement generic repositories and UoW**
4. **Update provider projects to inherit from base**
5. **Remove duplicated code**
6. **Update dependency injection**

This approach reduces code duplication by ~70% while maintaining provider-specific flexibility!
