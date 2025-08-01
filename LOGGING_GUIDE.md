# TechWayFit Licensing Management - Structured Logging Guide

## Overview

The application now uses structured logging with separate log files and correlation IDs for better traceability and debugging.

## Log Files Structure

The application creates the following log files in the `Logs/` directory:

### 1. `startup-YYYY-MM-DD.log`
- **Purpose**: Application startup and shutdown logs only
- **Content**: Bootstrap phase logs before full logging is configured
- **Includes**: Application start/stop messages, configuration loading
- **Retention**: 30 days

### 2. `sql-queries-YYYY-MM-DD.log`
- **Purpose**: All SQL queries executed by Entity Framework Core
- **Content**: 
  - SQL commands with parameters (from SqlLoggingInterceptor)
  - EF Core database command execution logs
  - Database validation messages
  - Execution duration and performance warnings
- **Excludes**: All other application logs
- **Retention**: 30 days

### 3. `requests-YYYY-MM-DD.log`
- **Purpose**: HTTP request/response logging only
- **Content**:
  - Request starting/finishing messages (Microsoft.AspNetCore.Hosting.Diagnostics)
  - Serilog request summary logs with timing and status codes
  - Request method, path, response status, duration
- **Excludes**: Controller actions, authentication, business logic
- **Retention**: 30 days

### 4. `application-YYYY-MM-DD.log`
- **Purpose**: Pure business logic and application-specific logs
- **Content**: 
  - Your custom application logs (controllers, services, business logic)
  - Authentication and authorization events
  - License operations, user management, etc.
- **Excludes**: SQL queries, HTTP requests, Microsoft framework logs
- **Retention**: 30 days

### 5. `errors-YYYY-MM-DD.log`
- **Purpose**: All error-level logs from any source
- **Content**: Errors, exceptions, and critical issues from all components
- **Retention**: 90 days

## Correlation ID System

### How It Works
- Each HTTP request gets assigned a unique correlation ID (GUID)
- The correlation ID is:
  - Generated automatically for new requests
  - Accepted from incoming `X-Correlation-ID` header if provided
  - Added to all log entries during request processing
  - Returned in the `X-Correlation-ID` response header

### Usage Examples

#### Client sends correlation ID:
```bash
curl -H "X-Correlation-ID: 12345678-1234-1234-1234-123456789012" \
     http://localhost:5000/api/products
```

#### Server generates correlation ID:
```bash
curl http://localhost:5000/api/products
# Response includes: X-Correlation-ID: a1b2c3d4-e5f6-7890-1234-567890abcdef
```

### Log Format with Correlation ID
```
[2025-08-01 10:30:45.123 +00:00 INF] [a1b2c3d4-e5f6-7890-1234-567890abcdef] User authentication successful for admin from 127.0.0.1
```

## Tracking Requests Across Log Files

To track a specific request across all log files, use the correlation ID:

### Linux/macOS:
```bash
# Search all log files for a specific correlation ID
grep "a1b2c3d4-e5f6-7890-1234-567890abcdef" Logs/*.log

# Search specific log file types
grep "a1b2c3d4-e5f6-7890-1234-567890abcdef" Logs/requests-*.log
grep "a1b2c3d4-e5f6-7890-1234-567890abcdef" Logs/sql-queries-*.log
```

### Windows PowerShell:
```powershell
# Search all log files for a specific correlation ID
Select-String "a1b2c3d4-e5f6-7890-1234-567890abcdef" Logs\*.log

# Search specific log file types
Select-String "a1b2c3d4-e5f6-7890-1234-567890abcdef" Logs\requests-*.log
```

## Configuration

### Application Settings
The logging configuration can be adjusted in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

### Log Levels
- **Debug**: Detailed diagnostic information
- **Information**: General application flow
- **Warning**: Potentially harmful situations
- **Error**: Error events that might still allow the application to continue
- **Fatal**: Very severe error events that presumably lead to application abort

## Performance Considerations

### SQL Query Performance
- Queries taking longer than 1000ms are logged as warnings
- All SQL queries include execution duration
- Parameters are logged (be careful with sensitive data in production)

### Log File Rotation
- Files rotate daily
- Old files are automatically cleaned up based on retention policies
- File size limits prevent excessive disk usage

## Development vs Production

### Development Environment
- More verbose logging
- SQL query parameters included
- Console output enabled
- Sensitive data logging enabled for EF Core

### Production Environment
- Reduced logging verbosity
- SQL parameters may be excluded
- Console output may be disabled
- Error-focused logging

## Troubleshooting

### Common Issues

1. **Missing Correlation IDs in logs**
   - Ensure `UseCorrelationId()` middleware is called early in the pipeline
   - Check that `Enrich.FromLogContext()` is configured in Serilog

2. **SQL queries not appearing in sql-queries.log**
   - Verify that `SqlLoggingInterceptor` is registered
   - Check Entity Framework logging level configuration

3. **Log files not created**
   - Ensure the application has write permissions to the Logs directory
   - Check that the Logs directory exists (created automatically on startup)

### Debugging Log Configuration
Enable Serilog self-logging in development:
```csharp
Serilog.Debugging.SelfLog.Enable(Console.Error);
```

## Best Practices

1. **Use Correlation IDs**: Always include correlation IDs when making API calls between services
2. **Structured Logging**: Use structured logging methods with named parameters
3. **Log Levels**: Use appropriate log levels for different types of information
4. **Sensitive Data**: Avoid logging sensitive information in production
5. **Performance**: Monitor log file sizes and adjust retention policies as needed

## Examples

### Business Logic Logging
```csharp
_logger.LogInformation("Processing license generation for product {ProductId} and user {UserId}", 
    productId, userId);
```

### Error Logging
```csharp
_logger.LogError(exception, "Failed to generate license for product {ProductId}: {ErrorMessage}", 
    productId, exception.Message);
```

### Performance Logging
```csharp
using var activity = _logger.BeginScope("License validation for {LicenseId}", licenseId);
var stopwatch = Stopwatch.StartNew();
// ... business logic ...
_logger.LogInformation("License validation completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```
