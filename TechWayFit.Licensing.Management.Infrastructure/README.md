# TechWayFit Licensing Infrastructure

This project contains the data access layer implementation for the TechWayFit Licensing system, including Entity Framework Core repositories, Unit of Work pattern, and PostgreSQL database support.

## Features

- **Complete Repository Pattern**: Generic base repository with domain-specific implementations
- **Unit of Work Pattern**: Coordinated operations with transaction support
- **Entity Framework Core**: Full ORM support with PostgreSQL optimization
- **AutoMapper Integration**: Domain model to database entity mapping
- **Audit Trail**: Automatic audit field management (CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
- **PostgreSQL Optimized**: Snake_case naming, JSONB support, indexes

## Project Structure

```
TechWayFit.Licensing.Infrastructure/
├── Contracts/                           # Repository interfaces
│   ├── ILicensingUnitOfWork.cs         # Unit of Work interface
│   └── Repositories/                    # Domain repository interfaces
│       ├── License/                     # License domain repositories
│       ├── Product/                     # Product domain repositories
│       ├── Consumer/                    # Consumer domain repositories
│       ├── Audit/                       # Audit domain repositories
│       └── Notification/                # Notification domain repositories
├── Data/                                # Implementation layer
│   ├── Context/                         # Entity Framework DbContext
│   ├── Entities/                        # Database entities
│   ├── Mappings/                        # AutoMapper profiles
│   ├── Repositories/                    # Repository implementations
│   └── UnitOfWork/                      # Unit of Work implementation
└── Extensions/                          # Dependency injection configuration
```

## Database Entities

### Core Domains

1. **License Domain**
   - `ProductLicenseEntity`: Main license records
   - `LicenseFeatureEntity`: Feature-specific license configurations

2. **Product Domain**
   - `ProductEntity`: Product definitions
   - `ProductVersionEntity`: Version management
   - `ProductTierEntity`: Pricing tiers
   - `ProductFeatureEntity`: Available features

3. **Consumer Domain**
   - `ConsumerAccountEntity`: Customer account management

4. **Audit Domain**
   - `AuditEntryEntity`: System audit trail

5. **Notification Domain**
   - `NotificationTemplateEntity`: Email/notification templates
   - `NotificationHistoryEntity`: Notification delivery tracking

## Configuration

### 1. Connection String

Add to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "LicensingDatabase": "Host=localhost;Database=licensing;Username=postgres;Password=password"
  }
}
```

### 2. Dependency Injection

Register the infrastructure services in your `Program.cs`:

```csharp
using TechWayFit.Licensing.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add licensing infrastructure with PostgreSQL (default)
builder.Services.AddLicensingInfrastructure(builder.Configuration);

// Or specify database provider explicitly
builder.Services.AddLicensingInfrastructure(
    builder.Configuration, 
    DatabaseProvider.PostgreSQL);

// Optional: Add database migration service
builder.Services.AddDatabaseMigrations();

var app = builder.Build();
```

### 3. Database Migrations

Apply database migrations:

```csharp
// In your application startup or separate migration tool
using var scope = app.Services.CreateScope();
var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();

// Check database connectivity
if (await migrationService.DatabaseExistsAsync())
{
    // Apply pending migrations
    await migrationService.MigrateAsync();
}
else
{
    // Create database and apply all migrations
    await migrationService.EnsureDatabaseCreatedAsync();
}
```

## Usage Examples

### 1. Using Unit of Work Pattern

```csharp
public class LicenseService
{
    private readonly ILicensingUnitOfWork _unitOfWork;

    public LicenseService(ILicensingUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CreateLicenseAsync(CreateLicenseRequest request)
    {
        return await _unitOfWork.ExecuteTransactionAsync(async uow =>
        {
            // Create consumer account if needed
            var account = await uow.ConsumerAccounts.GetByEmailAsync(request.Email);
            if (account == null)
            {
                account = new ConsumerAccountEntity
                {
                    AccountName = request.AccountName,
                    Email = request.Email,
                    // ... other properties
                };
                await uow.ConsumerAccounts.AddAsync(account);
            }

            // Create license
            var license = new ProductLicenseEntity
            {
                ConsumerAccountId = account.Id,
                ProductId = request.ProductId,
                LicenseKey = GenerateLicenseKey(),
                // ... other properties
            };
            await uow.ProductLicenses.AddAsync(license);

            // Create license features
            foreach (var featureId in request.FeatureIds)
            {
                var licenseFeature = new LicenseFeatureEntity
                {
                    ProductLicenseId = license.Id,
                    ProductFeatureId = featureId,
                    IsEnabled = true
                };
                await uow.LicenseFeatures.AddAsync(licenseFeature);
            }

            return true; // Transaction will be committed automatically
        });
    }
}
```

### 2. Using Individual Repositories

```csharp
public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductFeatureRepository _featureRepository;
    private readonly ILicensingUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        IProductFeatureRepository featureRepository,
        ILicensingUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _featureRepository = featureRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductEntity>> GetActiveProductsAsync()
    {
        return await _productRepository.GetActiveProductsAsync();
    }

    public async Task<ProductEntity?> GetProductWithDetailsAsync(Guid productId)
    {
        return await _productRepository.GetWithFullDetailsAsync(productId);
    }

    public async Task<bool> AddFeatureToProductAsync(Guid productId, CreateFeatureRequest request)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return false;

        var feature = new ProductFeatureEntity
        {
            ProductId = productId,
            Name = request.Name,
            FeatureCode = request.Code,
            FeatureType = request.Type,
            IsActive = true
        };

        await _featureRepository.AddAsync(feature);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }
}
```

### 3. Advanced Querying

```csharp
public class LicenseQueryService
{
    private readonly IProductLicenseRepository _licenseRepository;

    public LicenseQueryService(IProductLicenseRepository licenseRepository)
    {
        _licenseRepository = licenseRepository;
    }

    public async Task<(IEnumerable<ProductLicenseEntity>, int)> GetLicensesAsync(
        LicenseSearchCriteria criteria)
    {
        return await _licenseRepository.GetPagedByConsumerAccountAsync(
            criteria.ConsumerAccountId,
            criteria.PageNumber,
            criteria.PageSize,
            criteria.Status);
    }

    public async Task<IEnumerable<ProductLicenseEntity>> GetExpiringLicensesAsync(int daysFromNow)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysFromNow);
        return await _licenseRepository.GetExpiringLicensesAsync(expiryDate);
    }

    public async Task<bool> ValidateLicenseAsync(string licenseKey)
    {
        var license = await _licenseRepository.GetByLicenseKeyAsync(licenseKey);
        
        return license != null 
            && license.Status == LicenseStatus.Active
            && license.ValidFrom <= DateTime.UtcNow
            && license.ValidTo >= DateTime.UtcNow;
    }
}
```

## Database Schema

The infrastructure creates a PostgreSQL database with the following characteristics:

- **Naming Convention**: Snake_case for all database objects
- **Audit Fields**: All entities include CreatedBy, CreatedOn, UpdatedBy, UpdatedOn
- **Indexes**: Optimized indexes for common queries
- **Relationships**: Proper foreign key constraints with cascade behaviors
- **JSON Support**: JSONB columns for complex configuration data

## Best Practices

1. **Always use Unit of Work** for operations that involve multiple repositories
2. **Leverage the base repository** for common CRUD operations
3. **Use domain-specific repository methods** for complex queries
4. **Implement proper error handling** around database operations
5. **Use async/await patterns** for all database operations
6. **Consider using read-only contexts** for reporting queries

## Performance Considerations

- Repository methods include `AsNoTracking()` options for read-only scenarios
- Paged queries are implemented to prevent large result sets
- Includes are used judiciously to avoid N+1 queries
- Connection pooling is configured for optimal performance
- Retry policies are configured for transient failures

## Testing

For testing, you can use the in-memory database provider:

```csharp
services.AddDbContext<LicensingDbContext>(options =>
    options.UseInMemoryDatabase("TestDatabase"));
```

Or use a test PostgreSQL database with the same configuration.
