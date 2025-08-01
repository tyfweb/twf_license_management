# Operations Dashboard Data Collection Implementation

## Overview
This document summarizes the comprehensive middleware and background services implementation to capture real operational data for the Operations Dashboard in the TechWayFit Licensing Management Web Application.

## Implemented Components

### 1. Performance Tracking Middleware
**File**: `TechWayFit.Licensing.Management.Web/Middleware/PerformanceTrackingMiddleware.cs`

**Purpose**: Captures HTTP request performance metrics including response times, controller/action tracking, and error rates.

**Key Features**:
- ⏱️ Request timing with `Stopwatch` for accurate measurement  
- 🎯 Controller and action name extraction from route values
- 📊 HTTP method and status code tracking
- 🚫 Smart path filtering to avoid static files and recursion
- 📈 Integration with `IOperationsDashboardService.RecordPagePerformanceAsync()`
- 📉 System-wide metrics recording via `RecordSystemMetricAsync()`
- 🔧 Async processing to minimize performance impact

**Metrics Captured**:
- Response time (ms)
- Controller/Action details
- HTTP method and status
- User agent and IP address
- Request paths and parameters

### 2. Error Tracking Middleware
**File**: `TechWayFit.Licensing.Management.Web/Middleware/ErrorTrackingMiddleware.cs`

**Purpose**: Captures application errors, HTTP error responses (4xx/5xx), and unhandled exceptions.

**Key Features**:
- 🚨 Exception interception and recording
- 📊 HTTP error status tracking (400-599 range)
- 🎯 Error severity classification (Warning vs Error)
- 📝 Detailed error context including stack traces
- 🌐 Client information capture (IP, User-Agent)
- 🔄 Maintains normal error handling flow
- 📈 Integration with `IOperationsDashboardService.RecordErrorAsync()`

**Error Classifications**:
- **4xx responses**: Warning level
- **5xx responses**: Error level  
- **Exceptions**: Error level with stack traces

### 3. Enhanced SQL Interceptor
**File**: `TechWayFit.Licensing.Management.Web/Extensions/EnhancedSqlInterceptor.cs`

**Purpose**: Extends the existing SQL logging to capture query performance metrics for the operations dashboard.

**Key Features**:
- 📊 Query execution time tracking
- 🔍 Query type classification (SELECT, INSERT, UPDATE, DELETE, etc.)
- 🐌 Slow query detection (>1000ms threshold)
- 📝 Table name extraction from SQL commands
- 🚨 SQL error capture and recording
- 📈 Integration with `IOperationsDashboardService.RecordQueryPerformanceAsync()`
- 🔧 Works alongside existing `SqlLoggingInterceptor`

**Query Metrics**:
- Execution time (ms)
- Query type and table name
- Parameter count
- Slow query flagging
- Error details for failed queries

### 4. System Health Collection Service
**File**: `TechWayFit.Licensing.Management.Web/Services/SystemHealthCollectionService.cs`

**Purpose**: Background service that periodically collects comprehensive system health metrics.

**Key Features**:
- 🔄 Configurable collection interval (default: 30 seconds)
- 💾 Memory usage tracking (working set, GC metrics)
- 🖥️ CPU usage measurement
- 💿 Disk space monitoring
- 🔗 Database connection tracking
- 🧵 Thread count monitoring
- 🗑️ Garbage collection metrics
- 📊 Overall health score calculation
- 📈 Integration with `IOperationsDashboardService.RecordSystemHealthSnapshotAsync()`

**Health Metrics Collected**:
- CPU usage percentage
- Memory usage (working set, available memory)
- Disk usage and available space
- Active database connections
- Thread count
- GC collection counts (Gen 0, 1, 2)
- Overall health score (0-100)

## Configuration Updates

### appsettings.json
Added comprehensive configuration section for system health collection:

```json
"OperationsDashboard": {
  "SystemHealthCollection": {
    "IntervalSeconds": 30,
    "EnableCpuMonitoring": true,
    "EnableMemoryMonitoring": true,
    "EnableDiskMonitoring": true,
    "EnableDatabaseConnectionMonitoring": true,
    "EnableGcMonitoring": true
  }
}
```

### Program.cs Updates
**Service Registration**:
- `EnhancedSqlInterceptor` for SQL metrics
- `SystemHealthCollectionService` as hosted background service

**Middleware Pipeline**:
- `UsePerformanceTracking()` - Before routing for complete request coverage
- `UseErrorTracking()` - Before exception handling for comprehensive error capture

**Middleware Order**:
1. Correlation ID middleware
2. Serilog request logging  
3. **Performance tracking middleware**
4. **Error tracking middleware**
5. Static files
6. Routing
7. Authentication/Authorization

## Data Flow Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   HTTP Request  │───▶│ Performance      │───▶│ Operations          │
│                 │    │ Middleware       │    │ Dashboard Service   │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
                                │
                                ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   Exceptions    │───▶│ Error Tracking   │───▶│ Error Metrics       │
│   4xx/5xx       │    │ Middleware       │    │ Storage             │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
                                │
                                ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   SQL Queries   │───▶│ Enhanced SQL     │───▶│ Query Performance   │
│                 │    │ Interceptor      │    │ Metrics             │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
                                │
                                ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   Timer         │───▶│ System Health    │───▶│ Health Snapshots    │
│   (30s)         │    │ Service          │    │ & Metrics           │
└─────────────────┘    └──────────────────┘    └─────────────────────┘

                                │
                                ▼
                    ┌─────────────────────┐
                    │  PostgreSQL         │
                    │  Operations         │
                    │  Dashboard Tables   │
                    └─────────────────────┘
```

## Expected Data Capture

### Real-Time Metrics
- **HTTP Requests**: Response times, success/error rates, popular endpoints
- **Errors**: Exception details, error frequencies, error sources  
- **SQL Queries**: Query performance, slow queries, database activity
- **System Health**: CPU, memory, disk usage trends

### Dashboard Population
Once the application runs with user activity, the Operations Dashboard will display:

1. **Health Status**: Live system metrics and health scores
2. **Performance Metrics**: Request response times, slow endpoints  
3. **Error Tracking**: Error rates, exception details, failure patterns
4. **Query Performance**: Database query metrics, slow query identification
5. **Real-Time Data**: Live updating charts and metrics

## Testing & Validation

### Build Status
✅ **Build Successful** - All middlewares compile without errors

### Middleware Registration  
✅ **Performance Tracking** - Registered in pipeline
✅ **Error Tracking** - Registered in pipeline  
✅ **Enhanced SQL Interceptor** - Registered with DbContext
✅ **System Health Service** - Registered as background service

### Configuration
✅ **appsettings.json** - System health collection settings added
✅ **Service Dependencies** - All required services properly injected

## Next Steps

1. **Run the Application**: Start the web application to begin data collection
2. **Generate Activity**: Navigate through different pages to generate metrics
3. **Trigger Errors**: Test error scenarios to validate error tracking
4. **Monitor Dashboard**: Check Operations Dashboard for populated real data
5. **Performance Tuning**: Adjust collection intervals and thresholds as needed

## Maintenance & Monitoring

- **Log Files**: Check application logs for middleware operation status
- **Database Growth**: Monitor operations dashboard table sizes
- **Performance Impact**: Watch for any performance overhead from data collection
- **Configuration Tuning**: Adjust collection intervals based on system load

---

**Implementation Status**: ✅ Complete - All middlewares and background services implemented and ready for real-time data collection.
