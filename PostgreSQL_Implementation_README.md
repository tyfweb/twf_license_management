# PostgreSQL Repository Implementation

This document describes the PostgreSQL repository implementation for the TechWayFit Licensing System without Entity Framework, providing high-performance data access with proper security and maintainability.

## Overview

The PostgreSQL implementation provides:
- **High Performance**: Raw SQL queries optimized for PostgreSQL
- **Security**: Protection against SQL injection with parameterized queries
- **Scalability**: Connection pooling and efficient database operations
- **Maintainability**: Clean separation of concerns and comprehensive logging
- **Flexibility**: Support for complex queries and advanced PostgreSQL features

## Architecture

### Repository Pattern
```
IRepository (Interface)
    ↓
PostgreSqlBaseRepository (Base Class)
    ↓
Specific Repository Implementation
```

### Components

1. **PostgreSqlConfiguration**: Database connection configuration
2. **PostgreSqlBaseRepository**: Base class with common database operations
3. **Specific Repositories**: Implementation for each entity type
4. **ServiceCollectionExtensions**: Dependency injection setup
5. **InitializationService**: Database setup and health monitoring

## Repository Implementations

### 1. PostgreSqlConsumerRepository
- **Purpose**: Manages consumer (customer/organization) data
- **Features**: CRUD operations, search, filtering, pagination
- **Tables**: `consumers`

### 2. PostgreSqlProductRepository  
- **Purpose**: Manages product configurations, tiers, and features
- **Features**: Complex product management with related data
- **Tables**: `products`, `product_tiers`, `product_features`, `product_tier_features`

### 3. PostgreSqlLicenseRepository
- **Purpose**: Manages licenses with features and usage tracking
- **Features**: License lifecycle, validation tracking, usage metrics
- **Tables**: `licenses`, `license_features`, `license_validations`, `license_usage`

### 4. PostgreSqlLicenseRequestRepository
- **Purpose**: Manages license requests and approval workflow
- **Features**: Request lifecycle, status tracking, document management
- **Tables**: `license_requests`, `license_request_features`, `license_request_status_history`, `license_request_documents`

## Setup Instructions

### 1. Database Prerequisites
- PostgreSQL 12+ installed and running
- Database administrator access for initial setup

### 2. Database Setup
```bash
# Connect to PostgreSQL as superuser
psql -U postgres

# Run the setup script
\i setup_postgresql_database.sql
```

### 3. Application Configuration

#### Option A: Connection String
```json
{
  "PostgreSql": {
    "ConnectionString": "Host=localhost;Port=5432;Database=twf_license_management;Username=twf_license_user;Password=SecurePassword123!@#;SSL Mode=Prefer"
  }
}
```

#### Option B: Individual Parameters
```json
{
  "PostgreSql": {
    "Host": "localhost",
    "Port": 5432,
    "Database": "twf_license_management",
    "Username": "twf_license_user",
    "Password": "SecurePassword123!@#",
    "SslMode": "Prefer",
    "Pooling": true,
    "MinPoolSize": 5,
    "MaxPoolSize": 100,
    "ConnectionTimeout": 15,
    "CommandTimeout": 30
  }
}
```

### 4. Dependency Injection Setup

In `Program.cs` or `Startup.cs`:

```csharp
// Option 1: Using configuration
services.AddPostgreSqlRepositories(configuration);

// Option 2: Using connection string
services.AddPostgreSqlRepositories(connectionString);

// Option 3: Using individual parameters
services.AddPostgreSqlRepositories(
    host: "localhost",
    port: 5432,
    database: "twf_license_management",
    username: "twf_license_user",
    password: "SecurePassword123!@#"
);
```

### 5. Database Initialization

```csharp
// In application startup
var initService = serviceProvider.GetRequiredService<IPostgreSqlInitializationService>();
await initService.InitializeDatabaseAsync();
```

## Features

### Security Features
- **SQL Injection Protection**: All queries use parameterized statements
- **Connection Security**: SSL/TLS encryption support
- **Access Control**: Role-based database permissions
- **Audit Logging**: Comprehensive operation tracking

### Performance Features
- **Connection Pooling**: Efficient connection management
- **Optimized Queries**: Hand-tuned SQL for PostgreSQL
- **Indexes**: Proper indexing for fast queries
- **Bulk Operations**: Efficient batch processing

### Reliability Features
- **Transaction Support**: ACID compliance
- **Error Handling**: Comprehensive exception management
- **Retry Logic**: Configurable retry mechanisms
- **Health Monitoring**: Database health checks

### Monitoring Features
- **Metrics Collection**: Performance and usage metrics
- **Logging**: Detailed operation logging
- **Health Checks**: Database connectivity monitoring
- **Statistics**: Database performance statistics

## Database Schema

### Core Tables

#### Consumers
```sql
consumers (
    consumer_id VARCHAR(50) PRIMARY KEY,
    organization_name VARCHAR(200) NOT NULL,
    contact_person VARCHAR(100) NOT NULL,
    contact_email VARCHAR(150) NOT NULL,
    -- Additional fields...
)
```

#### Products
```sql
products (
    product_id VARCHAR(50) PRIMARY KEY,
    product_name VARCHAR(200) NOT NULL,
    product_type VARCHAR(50) NOT NULL,
    version VARCHAR(50) NOT NULL,
    -- Additional fields...
)
```

#### Licenses
```sql
licenses (
    license_id VARCHAR(50) PRIMARY KEY,
    consumer_id VARCHAR(50) NOT NULL,
    product_id VARCHAR(50) NOT NULL,
    tier_id VARCHAR(50) NOT NULL,
    valid_from TIMESTAMP WITH TIME ZONE NOT NULL,
    valid_to TIMESTAMP WITH TIME ZONE NOT NULL,
    -- Additional fields...
)
```

#### License Requests
```sql
license_requests (
    request_id VARCHAR(50) PRIMARY KEY,
    consumer_id VARCHAR(50) NOT NULL,
    product_id VARCHAR(50) NOT NULL,
    requested_tier VARCHAR(50) NOT NULL,
    request_status VARCHAR(20) NOT NULL,
    -- Additional fields...
)
```

## Usage Examples

### Basic Repository Usage
```csharp
public class LicenseService
{
    private readonly ILicenseRepository _licenseRepository;
    
    public LicenseService(ILicenseRepository licenseRepository)
    {
        _licenseRepository = licenseRepository;
    }
    
    public async Task<License> CreateLicenseAsync(License license)
    {
        return await _licenseRepository.CreateLicenseAsync(license);
    }
    
    public async Task<IEnumerable<License>> GetActiveLicensesAsync(string consumerId, string productId)
    {
        return await _licenseRepository.GetActiveLicensesAsync(consumerId, productId);
    }
}
```

### Advanced Querying
```csharp
// Get licenses with filtering and pagination
var licenses = await _licenseRepository.GetAllLicensesAsync(
    consumerId: "consumer-123",
    productId: "api-gateway",
    status: LicenseStatus.Active,
    pageNumber: 1,
    pageSize: 10
);

// Get license usage statistics
await _licenseRepository.UpdateUsageAsync(
    licenseId: "license-456",
    usageDate: DateTime.Today,
    apiCalls: 1000,
    concurrentConnections: 5
);
```

### Health Monitoring
```csharp
var healthService = serviceProvider.GetRequiredService<IPostgreSqlInitializationService>();
var healthInfo = await healthService.GetDatabaseHealthAsync();

Console.WriteLine($"Database Connected: {healthInfo.IsConnected}");
Console.WriteLine($"Active Connections: {healthInfo.ActiveConnections}");
Console.WriteLine($"Connection Time: {healthInfo.ConnectionTime.TotalMilliseconds}ms");
```

## Migration from In-Memory to PostgreSQL

### 1. Update Dependencies
Remove in-memory repository registrations and add PostgreSQL:
```csharp
// Remove
// services.AddScoped<IConsumerRepository, InMemoryConsumerRepository>();

// Add
services.AddPostgreSqlRepositories(configuration);
```

### 2. Data Migration
```csharp
// Export existing data (if any) and import to PostgreSQL
var existingConsumers = await oldRepository.GetAllConsumersAsync();
foreach (var consumer in existingConsumers)
{
    await newRepository.CreateConsumerAsync(consumer);
}
```

### 3. Configuration Update
Update `appsettings.json` with PostgreSQL connection details.

### 4. Testing
Verify all functionality works with the new repository implementation.

## Performance Tuning

### Database Configuration
- **shared_buffers**: 25% of total RAM
- **work_mem**: 4MB for complex queries
- **max_connections**: Based on application needs
- **effective_cache_size**: 75% of total RAM

### Application Configuration
- **Connection Pooling**: Enable with appropriate min/max pool sizes
- **Command Timeout**: Set based on query complexity
- **Connection Timeout**: Set for responsive failure detection

### Monitoring Queries
```sql
-- Check slow queries
SELECT query, mean_exec_time, calls
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;

-- Check connection usage
SELECT count(*), state
FROM pg_stat_activity
WHERE datname = 'twf_license_management'
GROUP BY state;
```

## Troubleshooting

### Common Issues

1. **Connection Failures**
   - Check PostgreSQL service status
   - Verify connection string parameters
   - Check firewall settings

2. **Permission Errors**
   - Verify user permissions on database and tables
   - Check role assignments

3. **Performance Issues**
   - Review query execution plans
   - Check for missing indexes
   - Monitor connection pool usage

4. **Transaction Deadlocks**
   - Review transaction isolation levels
   - Optimize query order
   - Implement retry logic

### Logging
Enable detailed logging for troubleshooting:
```json
{
  "Logging": {
    "LogLevel": {
      "TechWayFit.Licensing": "Debug",
      "Npgsql": "Information"
    }
  }
}
```

## Best Practices

### Security
- Use connection pooling to prevent connection exhaustion
- Always use parameterized queries
- Implement proper error handling without exposing sensitive information
- Use SSL/TLS for database connections
- Regularly update PostgreSQL and Npgsql packages

### Performance
- Use appropriate indexes for query patterns
- Implement pagination for large result sets
- Use connection pooling efficiently
- Monitor and optimize slow queries
- Use appropriate transaction isolation levels

### Maintainability
- Keep database schema migrations in version control
- Document complex queries and business logic
- Use consistent naming conventions
- Implement comprehensive logging
- Regular database maintenance and monitoring

## Support

For issues related to the PostgreSQL implementation:
1. Check the logs for detailed error information
2. Verify database connectivity and permissions
3. Review the configuration settings
4. Check PostgreSQL server status and resources
5. Consult the PostgreSQL documentation for database-specific issues

## Future Enhancements

Planned improvements include:
- Connection string encryption
- Advanced caching strategies
- Read/write replica support
- Database sharding capabilities
- Enhanced monitoring and alerting
- Automated backup and recovery procedures
