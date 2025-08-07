# TechWayFit Licensing Management Infrastructure - SQL Server

This project provides SQL Server database provider implementation for the TechWayFit licensing management system using Entity Framework Core.

## Overview

The SQL Server infrastructure project extends the provider-agnostic Entity Framework base to provide SQL Server-specific functionality including:

- SQL Server database provider configuration
- Connection string management and validation
- Migration and database initialization helpers
- Performance optimization settings
- Production-ready connection pooling
- Retry policies for transient failures

## Features

### ✅ Core Functionality
- **SQL Server Provider**: Complete Entity Framework Core SQL Server integration
- **Connection Management**: Advanced connection string building and validation
- **Migration Support**: Automatic database migrations and initialization
- **Performance Optimization**: Connection pooling, retry policies, command timeouts
- **Configuration**: Comprehensive configuration options through appsettings.json

### ✅ Development Features
- **Multiple Registration Methods**: Various ways to register SQL Server services
- **Testing Support**: In-memory database fallback for testing scenarios
- **Diagnostics**: Connection validation and server information retrieval
- **Logging**: Comprehensive logging throughout the infrastructure

## Quick Start

### 1. Install Dependencies

The project includes all necessary Entity Framework Core SQL Server packages:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
```

### 2. Configuration

Add SQL Server configuration to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LicensingDb;Integrated Security=true;TrustServerCertificate=true;"
  },
  "SqlServer": {
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:05",
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "ConnectionPool": {
      "MaxPoolSize": 100,
      "MinPoolSize": 0,
      "ConnectionTimeout": 15
    },
    "Performance": {
      "EnableQueryLogging": false,
      "SlowQueryThresholdMs": 1000
    }
  }
}
```

### 3. Service Registration

#### Option A: Simple Registration (Recommended)
```csharp
// Program.cs or Startup.cs
services.AddSqlServerInfrastructure(configuration);
```

#### Option B: Custom Connection String
```csharp
services.AddSqlServerInfrastructure(
    "Server=localhost;Database=LicensingDb;Integrated Security=true;");
```

#### Option C: Advanced Configuration
```csharp
services.AddSqlServerInfrastructureWithOptions(
    connectionString: configuration.GetConnectionString("DefaultConnection"),
    retryCount: 3,
    maxRetryDelay: TimeSpan.FromSeconds(5),
    commandTimeout: 30,
    enableSensitiveDataLogging: false,
    enableDetailedErrors: false);
```

#### Option D: Testing Configuration
```csharp
services.AddSqlServerInfrastructureForTesting(
    connectionString: testConnectionString,
    useInMemoryForTesting: true, // Fallback to in-memory for tests
    testDatabaseName: "TestLicensingDb");
```

### 4. Database Initialization

```csharp
// In your application startup
using TechWayFit.Licensing.Management.Infrastructure.SqlServer.Helpers;

// Initialize database with migrations
await SqlServerDatabaseHelper.InitializeDatabaseAsync(
    serviceProvider,
    applyMigrations: true,
    seedData: false,
    logger);

// Or individual operations
await SqlServerDatabaseHelper.EnsureDatabaseCreatedAsync(serviceProvider, logger);
await SqlServerDatabaseHelper.ApplyMigrationsAsync(serviceProvider, logger);
```

## Advanced Usage

### Connection String Building

```csharp
using TechWayFit.Licensing.Management.Infrastructure.SqlServer.Helpers;

// Build connection string programmatically
var connectionString = SqlServerConnectionHelper.BuildConnectionString(
    server: "localhost",
    database: "LicensingDb",
    integratedSecurity: true,
    connectionTimeout: 15,
    encrypt: true,
    trustServerCertificate: true);

// Validate connection
var isValid = await SqlServerConnectionHelper.ValidateConnectionAsync(
    connectionString, logger);

// Get server information
var serverInfo = await SqlServerConnectionHelper.GetServerInfoAsync(
    connectionString, logger);
```

### Migration Management

```csharp
// Get migration status
var migrationInfo = await SqlServerDatabaseHelper.GetMigrationInfoAsync(
    serviceProvider, logger);

Console.WriteLine($"Applied: {migrationInfo.AppliedMigrations.Count}");
Console.WriteLine($"Pending: {migrationInfo.PendingMigrations.Count}");
Console.WriteLine($"Can Connect: {migrationInfo.CanConnect}");
```

### Custom Configuration

```csharp
services.AddSqlServerInfrastructure(connectionString, options =>
{
    // Add custom Entity Framework options
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine);
    
    // Add interceptors, conventions, etc.
});
```

## Configuration Options

### SqlServerOptions

| Property | Description | Default |
|----------|-------------|---------|
| `ConnectionString` | SQL Server connection string | "" |
| `MaxRetryCount` | Maximum retry attempts for transient failures | 3 |
| `MaxRetryDelay` | Maximum delay between retries | 5 seconds |
| `CommandTimeout` | Command timeout in seconds | 30 |
| `EnableSensitiveDataLogging` | Log sensitive data (dev only) | false |
| `EnableDetailedErrors` | Enable detailed error messages | false |

### Connection Pool Options

| Property | Description | Default |
|----------|-------------|---------|
| `MaxPoolSize` | Maximum connection pool size | 100 |
| `MinPoolSize` | Minimum connection pool size | 0 |
| `ConnectionTimeout` | Connection timeout in seconds | 15 |
| `ConnectionLifetime` | Connection lifetime in seconds | 0 |

## Best Practices

### Production Deployment

1. **Use Connection Pooling**: Configure appropriate pool sizes
2. **Enable Retry Policies**: Handle transient failures gracefully
3. **Monitor Performance**: Use query logging for slow queries
4. **Secure Connections**: Always use encryption in production
5. **Optimize Timeouts**: Set appropriate command and connection timeouts

### Development

1. **Use Local SQL Server**: SQL Server Express or LocalDB
2. **Enable Detailed Logging**: For debugging Entity Framework issues
3. **Use Migrations**: Always use migrations for schema changes
4. **Test with Real Database**: Use SQL Server for integration tests

### Testing

1. **In-Memory Fallback**: Use in-memory provider for unit tests
2. **Test Database**: Use separate database for integration tests
3. **Transaction Rollback**: Use transactions that rollback for test isolation

## Architecture Integration

This SQL Server implementation integrates with the broader licensing management architecture:

```
┌─────────────────────────────────────────────────┐
│                 Web Layer                       │
├─────────────────────────────────────────────────┤
│                Services Layer                   │
├─────────────────────────────────────────────────┤
│            Infrastructure Layer                 │
│  ┌─────────────────────────────────────────────┐│
│  │     Infrastructure.SqlServer (This)        ││
│  │  ┌─────────────────────────────────────────┐││
│  │  │    Infrastructure.EntityFramework      │││
│  │  │             (Base)                     │││
│  │  └─────────────────────────────────────────┘││
│  └─────────────────────────────────────────────┘│
├─────────────────────────────────────────────────┤
│                 Core Layer                      │
└─────────────────────────────────────────────────┘
```

## Troubleshooting

### Common Issues

1. **Connection Failures**
   - Verify SQL Server is running
   - Check connection string format
   - Ensure network connectivity
   - Validate credentials

2. **Migration Errors**
   - Check database permissions
   - Verify migration files exist
   - Ensure unique migration names

3. **Performance Issues**
   - Monitor connection pool usage
   - Check for long-running queries
   - Review retry policy settings
   - Optimize database indexes

### Debugging

```csharp
// Enable detailed logging
services.AddSqlServerInfrastructureWithOptions(
    connectionString,
    enableSensitiveDataLogging: true,
    enableDetailedErrors: true,
    configureOptions: options =>
    {
        options.LogTo(Console.WriteLine, LogLevel.Information);
    });
```

## Related Projects

- **TechWayFit.Licensing.Management.Infrastructure.EntityFramework**: Provider-agnostic EF Core base
- **TechWayFit.Licensing.Management.Infrastructure.InMemory**: In-memory provider for testing
- **TechWayFit.Licensing.Management.Infrastructure.PostgreSql**: PostgreSQL provider implementation

## Contributing

When extending this SQL Server implementation:

1. Follow the established patterns from the EntityFramework base
2. Add comprehensive logging for diagnostics
3. Include proper error handling and retry logic
4. Update this README with new features
5. Add unit tests for new functionality

## License

This project is part of the TechWayFit Licensing Management System.
