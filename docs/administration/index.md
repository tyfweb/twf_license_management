---
title: Administration
nav_order: 6
has_children: true
---

# System Administration
{: .no_toc }

Complete guide for administering and maintaining the TechWayFit License Management System.

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

---

## Overview

This section covers all aspects of system administration, including installation, configuration, security, monitoring, and maintenance of the TechWayFit License Management System.

## Administrator Responsibilities

### System Administrator
- **Infrastructure Management**: Server setup, database administration, backup strategies
- **Security Management**: User access control, encryption key management, security audits
- **Performance Monitoring**: System health, performance optimization, capacity planning
- **Maintenance**: Updates, patches, database maintenance, troubleshooting

### Tenant Administrator
- **User Management**: Managing users within their tenant boundary
- **Product Configuration**: Setting up products and licensing models
- **Consumer Management**: Managing customer accounts and relationships
- **Reporting**: Access to tenant-specific analytics and reports

---

## System Configuration

### Application Settings

The system configuration is managed through `appsettings.json` files:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=licensing;Username=admin;Password=secure_password"
  },
  "Infrastructure": {
    "Provider": "PostgreSql",
    "MigrationsAssembly": "TechWayFit.Licensing.Management.Infrastructure.PostgreSql"
  },
  "Security": {
    "EncryptionKey": "base64_encoded_key",
    "JwtSettings": {
      "SecretKey": "jwt_secret_key",
      "Issuer": "TechWayFit.Licensing",
      "Audience": "TechWayFit.Licensing.Users",
      "ExpirationMinutes": 60
    },
    "Lockout": {
      "DefaultLockoutTimeSpan": "00:05:00",
      "MaxFailedAccessAttempts": 5,
      "AllowedForNewUsers": true
    }
  },
  "Caching": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "Database": 0,
      "KeyPrefix": "licensing:"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "TechWayFit.Licensing": "Debug"
    },
    "Serilog": {
      "MinimumLevel": "Information",
      "WriteTo": [
        {
          "Name": "Console"
        },
        {
          "Name": "File",
          "Args": {
            "path": "logs/licensing-.log",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30
          }
        }
      ]
    }
  },
  "Email": {
    "SmtpServer": "smtp.company.com",
    "Port": 587,
    "Username": "licensing@company.com",
    "Password": "smtp_password",
    "FromAddress": "licensing@company.com",
    "FromName": "TechWayFit Licensing"
  },
  "Hangfire": {
    "Dashboard": {
      "Enabled": true,
      "Path": "/hangfire",
      "Authorization": "Administrators"
    }
  }
}
```

### Environment-Specific Configuration

#### Development
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "TechWayFit.Licensing": "Trace"
    }
  },
  "Security": {
    "RequireHttps": false,
    "EnableSwagger": true
  }
}
```

#### Production
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "TechWayFit.Licensing": "Information"
    }
  },
  "Security": {
    "RequireHttps": true,
    "EnableSwagger": false,
    "HstsOptions": {
      "IncludeSubDomains": true,
      "MaxAge": "365.00:00:00"
    }
  }
}
```

---

## Database Administration

### Database Providers

The system supports multiple database providers:

#### PostgreSQL (Recommended)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db-server;Port=5432;Database=licensing;Username=licensing_user;Password=secure_password;SSL Mode=Require"
  },
  "Infrastructure": {
    "Provider": "PostgreSql"
  }
}
```

Benefits:
- Excellent performance for read-heavy workloads
- Advanced features (JSON support, full-text search)
- Strong ACID compliance
- Active community support

#### SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server;Database=Licensing;User Id=licensing_user;Password=secure_password;Encrypt=true;TrustServerCertificate=false"
  },
  "Infrastructure": {
    "Provider": "SqlServer"
  }
}
```

Benefits:
- Enterprise features (advanced analytics, reporting)
- Excellent tooling and management
- Integration with Microsoft ecosystem
- Advanced security features

### Database Migrations

#### Apply Migrations
```bash
# Navigate to web project
cd source/website/TechWayFit.Licensing.Management.Web

# Apply all pending migrations
dotnet ef database update

# Apply specific migration
dotnet ef database update Migration_Name

# Generate SQL script for review
dotnet ef migrations script --idempotent --output migration.sql
```

#### Create New Migration
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Database Maintenance

#### Backup Strategy
```sql
-- PostgreSQL backup
pg_dump -h localhost -U postgres -d licensing > licensing_backup_$(date +%Y%m%d).sql

-- SQL Server backup
BACKUP DATABASE [Licensing] 
TO DISK = 'C:\Backups\Licensing_Full_$(date).bak'
WITH FORMAT, INIT, NAME = 'Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10
```

#### Index Optimization
```sql
-- PostgreSQL index analysis
SELECT schemaname, tablename, attname, n_distinct, correlation 
FROM pg_stats 
WHERE tablename IN ('products', 'product_licenses', 'consumer_accounts');

-- Rebuild indexes if needed
REINDEX TABLE product_licenses;

-- SQL Server index maintenance
ALTER INDEX ALL ON product_licenses REBUILD;
EXEC sp_updatestats;
```

#### Performance Monitoring
```sql
-- PostgreSQL slow query monitoring
SELECT query, calls, total_time, mean_time
FROM pg_stat_statements
ORDER BY total_time DESC
LIMIT 10;

-- SQL Server query performance
SELECT TOP 10
    qs.sql_handle,
    qs.execution_count,
    qs.total_worker_time as total_cpu_time,
    qs.total_elapsed_time,
    SUBSTRING(qt.text, qs.statement_start_offset/2+1,
        (CASE WHEN qs.statement_end_offset = -1
            THEN LEN(CONVERT(nvarchar(max), qt.text)) * 2
            ELSE qs.statement_end_offset end - qs.statement_start_offset)/2 + 1) as query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) as qt
ORDER BY qs.total_worker_time DESC;
```

---

## Security Administration

### Encryption Key Management

#### RSA Key Generation
```bash
# Generate new RSA key pair for license signing
openssl genrsa -out private.key 2048
openssl rsa -in private.key -pubout -out public.key

# Convert to base64 for configuration
base64 -w 0 private.key > private.key.b64
base64 -w 0 public.key > public.key.b64
```

#### Key Rotation Process
1. **Generate New Keys**: Create new RSA key pair
2. **Update Configuration**: Add new keys alongside existing ones
3. **Deploy Update**: Deploy application with new configuration
4. **Verify Operation**: Ensure new licenses use new keys
5. **Revoke Old Keys**: Remove old keys after transition period

### User Management

#### User Roles and Permissions
```csharp
public static class Roles
{
    public const string SystemAdministrator = "SystemAdministrator";
    public const string TenantAdministrator = "TenantAdministrator";
    public const string LicenseManager = "LicenseManager";
    public const string ConsumerManager = "ConsumerManager";
    public const string ReadOnlyUser = "ReadOnlyUser";
}

public static class Permissions
{
    public const string ManageProducts = "manage:products";
    public const string ManageLicenses = "manage:licenses";
    public const string ManageConsumers = "manage:consumers";
    public const string ViewReports = "view:reports";
    public const string ManageUsers = "manage:users";
    public const string SystemConfiguration = "system:configuration";
}
```

#### Creating System Administrator
```bash
# Using CLI tool (if available)
dotnet run -- create-admin \
  --email admin@company.com \
  --password SecurePassword123! \
  --first-name "System" \
  --last-name "Administrator"

# Or via API
curl -X POST "https://api.licensing.com/api/v1/admin/users" \
  -H "Authorization: Bearer SYSTEM_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@company.com",
    "password": "SecurePassword123!",
    "firstName": "System",
    "lastName": "Administrator",
    "roles": ["SystemAdministrator"]
  }'
```

### SSL/TLS Configuration

#### Certificate Installation
```bash
# Generate self-signed certificate (development only)
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# Convert for .NET applications
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem

# Install certificate in application
dotnet dev-certs https --clean
dotnet dev-certs https --trust -v
```

#### HTTPS Configuration
```csharp
// Program.cs
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443;
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});
```

---

## Monitoring and Logging

### Application Monitoring

#### Health Checks
```csharp
// Configure health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LicensingDbContext>()
    .AddRedis(connectionString)
    .AddSmtpHealthCheck(options =>
    {
        options.Host = "smtp.company.com";
        options.Port = 587;
    });

// Health check endpoint: /health
// Detailed health check: /health/detailed
```

#### Application Insights Integration
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true,
    "EnableRequestTrackingTelemetryModule": true,
    "EnableDependencyTrackingTelemetryModule": true
  }
}
```

### Logging Configuration

#### Structured Logging with Serilog
```csharp
// Program.cs
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TechWayFit.Licensing")
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.File(
            path: "logs/licensing-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            formatter: new JsonFormatter())
        .WriteTo.ApplicationInsights(
            context.Configuration.GetConnectionString("ApplicationInsights"),
            TelemetryConverter.Traces);
});
```

#### Log Analysis Queries
```sql
-- Most common errors (Application Insights)
traces
| where timestamp > ago(24h)
| where severityLevel >= 3
| summarize count() by operation_Name, message
| order by count_ desc
| take 10

-- Performance issues
requests
| where timestamp > ago(1h)
| where duration > 5000
| project timestamp, name, duration, resultCode
| order by duration desc
```

### Performance Monitoring

#### Key Performance Indicators (KPIs)
- **Response Time**: 95th percentile < 500ms
- **Availability**: > 99.9% uptime
- **Error Rate**: < 0.1% of requests
- **Database Performance**: Average query time < 100ms
- **License Validation**: < 50ms response time

#### Performance Dashboards
```json
{
  "dashboards": [
    {
      "name": "System Overview",
      "widgets": [
        "response_time_chart",
        "error_rate_gauge",
        "active_users_counter",
        "license_validations_per_minute"
      ]
    },
    {
      "name": "Database Performance",
      "widgets": [
        "query_performance_chart",
        "database_connections_gauge",
        "slow_queries_table"
      ]
    }
  ]
}
```

---

## Backup and Recovery

### Backup Strategy

#### Automated Backups
```bash
#!/bin/bash
# backup-script.sh

BACKUP_DIR="/backups/licensing"
DATE=$(date +%Y%m%d_%H%M%S)

# Database backup
pg_dump -h db-server -U licensing_user -d licensing > "$BACKUP_DIR/db_backup_$DATE.sql"

# Application files backup
tar -czf "$BACKUP_DIR/app_backup_$DATE.tar.gz" \
  /var/www/licensing \
  --exclude="*/logs/*" \
  --exclude="*/temp/*"

# Configuration backup
cp /etc/licensing/appsettings.Production.json "$BACKUP_DIR/config_backup_$DATE.json"

# Cleanup old backups (keep 30 days)
find "$BACKUP_DIR" -name "*backup*" -mtime +30 -delete

# Upload to cloud storage (optional)
aws s3 sync "$BACKUP_DIR" s3://licensing-backups/$(date +%Y/%m/%d)/
```

#### Backup Verification
```bash
#!/bin/bash
# verify-backup.sh

BACKUP_FILE="$1"
TEST_DB="licensing_test_restore"

# Test database restore
createdb "$TEST_DB"
psql -d "$TEST_DB" -f "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    echo "Backup verification successful"
    dropdb "$TEST_DB"
    exit 0
else
    echo "Backup verification failed"
    exit 1
fi
```

### Disaster Recovery

#### Recovery Time Objectives (RTO)
- **Critical Systems**: 1 hour
- **License Validation**: 15 minutes
- **Web Interface**: 2 hours
- **Reporting Systems**: 4 hours

#### Recovery Point Objectives (RPO)
- **Transaction Data**: 5 minutes
- **Configuration Data**: 1 hour
- **Log Data**: 1 hour

#### Recovery Procedures
1. **Assess Damage**: Determine scope of data loss
2. **Restore Database**: From most recent valid backup
3. **Restore Application**: Deploy from known good version
4. **Verify Integrity**: Run data validation checks
5. **Resume Operations**: Gradual service restoration
6. **Post-Incident Review**: Document lessons learned

---

## System Maintenance

### Update Procedures

#### Application Updates
```bash
# 1. Backup current system
./backup-script.sh

# 2. Stop application
systemctl stop licensing

# 3. Deploy new version
cp -r new-release/* /var/www/licensing/

# 4. Run database migrations
cd /var/www/licensing
dotnet ef database update

# 5. Update configuration if needed
cp new-appsettings.json /etc/licensing/

# 6. Start application
systemctl start licensing

# 7. Verify deployment
curl -f http://localhost/health || echo "Deployment failed"
```

#### Database Maintenance
```sql
-- Weekly maintenance script
-- Rebuild fragmented indexes
SELECT schemaname, tablename, indexname 
FROM pg_indexes 
WHERE schemaname = 'public';

-- Update table statistics
ANALYZE;

-- Clean up old audit logs (keep 1 year)
DELETE FROM audit_logs 
WHERE created_at < NOW() - INTERVAL '1 year';

-- Vacuum tables
VACUUM ANALYZE;
```

### Capacity Planning

#### Resource Monitoring
```bash
# System resource monitoring script
#!/bin/bash

echo "=== System Resources ==="
echo "CPU Usage: $(top -bn1 | grep "Cpu(s)" | awk '{print $2}' | cut -d'%' -f1)"
echo "Memory Usage: $(free -m | awk 'NR==2{printf "%.1f%%", $3*100/$2}')"
echo "Disk Usage: $(df -h / | awk 'NR==2{print $5}')"

echo "=== Database Connections ==="
psql -d licensing -c "SELECT count(*) as active_connections FROM pg_stat_activity WHERE state = 'active';"

echo "=== Application Metrics ==="
curl -s http://localhost/metrics | grep -E "(http_requests_total|database_queries_total)"
```

#### Growth Projections
| Metric | Current | 6 Months | 1 Year |
|--------|---------|----------|--------|
| Users | 100 | 250 | 500 |
| Products | 20 | 40 | 80 |
| Licenses | 1,000 | 5,000 | 15,000 |
| API Calls/Day | 10,000 | 50,000 | 150,000 |
| Database Size | 1GB | 5GB | 15GB |

---

## Troubleshooting

### Common Issues

#### Application Won't Start
```bash
# Check logs
journalctl -u licensing -f

# Common issues:
# 1. Database connection
psql -h db-server -U licensing_user -d licensing -c "SELECT 1;"

# 2. Port conflicts
netstat -tulpn | grep :5000

# 3. Permission issues
ls -la /var/www/licensing/
chown -R www-data:www-data /var/www/licensing/
```

#### Performance Issues
```sql
-- Check for blocking queries
SELECT pid, now() - pg_stat_activity.query_start AS duration, query 
FROM pg_stat_activity 
WHERE (now() - pg_stat_activity.query_start) > interval '5 minutes';

-- Check table sizes
SELECT schemaname, tablename, pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
FROM pg_tables 
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

#### License Validation Failures
```bash
# Check encryption keys
openssl rsa -in private.key -check
openssl rsa -in private.key -pubout | openssl md5

# Verify license file format
file license.lic
hexdump -C license.lic | head

# Test validation manually
curl -X POST "http://localhost/api/v1/licenses/validate" \
  -H "Content-Type: application/json" \
  -d '{"licenseKey": "TEST-KEY"}'
```

---

## Security Auditing

### Audit Logging
All system activities are logged for security auditing:

```csharp
public class AuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Changes { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid TenantId { get; set; }
}
```

### Security Monitoring
```sql
-- Failed login attempts
SELECT ip_address, count(*) as attempts
FROM audit_logs
WHERE action = 'LOGIN_FAILED'
  AND timestamp > NOW() - INTERVAL '1 hour'
GROUP BY ip_address
HAVING count(*) > 5;

-- Unusual license validation patterns
SELECT ip_address, count(*) as validations
FROM audit_logs
WHERE action = 'LICENSE_VALIDATED'
  AND timestamp > NOW() - INTERVAL '1 hour'
GROUP BY ip_address
HAVING count(*) > 1000;
```

### Compliance Reports
Generate regular compliance reports for auditors:

```bash
# Monthly security report
python generate_security_report.py \
  --month $(date +%Y-%m) \
  --output monthly_security_report.pdf \
  --include-failed-logins \
  --include-privilege-changes \
  --include-data-access
```

---

## Next Steps

For detailed information on specific administrative tasks:

1. **[Multi-Tenant Configuration](multi-tenant.html)** - Setting up and managing multiple tenants
2. **[Performance Tuning](performance.html)** - Optimizing system performance
3. **[Security Hardening](security.html)** - Advanced security configuration
4. **[Backup and Recovery](backup-recovery.html)** - Detailed backup strategies
5. **[API Management](api-management.html)** - Managing API access and rate limiting
