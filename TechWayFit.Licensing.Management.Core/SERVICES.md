# Services Documentation

## Overview

This document provides comprehensive specifications for all service interfaces in the TechWayFit.Licensing.Management.Core project. These interfaces define the business operations and form the contract layer for the licensing management system.

## Service Architecture

### Design Principles

- **Interface Segregation** - Each service has a focused responsibility
- **Dependency Inversion** - Services depend on abstractions, not implementations
- **Async-First** - All operations are asynchronous for scalability
- **Validation-Centric** - Comprehensive validation at service boundaries
- **Audit-Enabled** - Built-in audit trail support

### Service Categories

| Category | Services | Purpose |
|----------|----------|---------|
| **Core Business** | License, Product, Consumer | Primary business operations |
| **Supporting** | Tier, Feature | Product hierarchy management |
| **Infrastructure** | Cryptographic, Audit | Security and compliance |
| **User Experience** | Notification, Reporting | User interaction and insights |

## Core Business Services

### IProductLicenseService

**Purpose**: Complete license lifecycle management with cryptographic security.

#### Key Operations

**License Generation**
```csharp
Task<ProductLicense> GenerateLicenseAsync(LicenseGenerationRequest request, string generatedBy)
```
- Generates new licenses from request parameters
- Integrates with cryptographic service for secure key generation
- Validates business rules before generation

**License Validation**
```csharp
Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, string productId, bool checkActivation = true)
```
- Cryptographic validation of license keys
- Product compatibility verification
- Activation status checking

**License Lifecycle**
```csharp
Task<bool> ActivateLicenseAsync(string licenseKey, ActivationInfo activationInfo)
Task<bool> DeactivateLicenseAsync(string licenseKey, string deactivatedBy, string? reason = null)
Task<bool> RenewLicenseAsync(string licenseId, DateTime newExpiryDate, string renewedBy)
Task<bool> RevokeLicenseAsync(string licenseId, string revokedBy, string reason)
Task<bool> SuspendLicenseAsync(string licenseId, string suspendedBy, string reason, DateTime? suspensionEndDate = null)
```

**License Management**
```csharp
Task<ProductLicense> UpdateLicenseAsync(string licenseId, LicenseUpdateRequest request, string updatedBy)
Task<ProductLicense> RegenerateLicenseKeyAsync(string licenseId, string regeneratedBy, string reason)
```

**Query Operations**
```csharp
Task<IEnumerable<ProductLicense>> GetLicensesByConsumerAsync(string consumerId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<ProductLicense>> GetLicensesByProductAsync(string productId, LicenseStatus? status = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<ProductLicense>> GetExpiringLicensesAsync(int daysFromNow = 30)
Task<IEnumerable<ProductLicense>> GetExpiredLicensesAsync()
```

**Analytics & Audit**
```csharp
Task<LicenseUsageStatistics> GetLicenseUsageStatisticsAsync(string? productId = null, string? consumerId = null)
Task<IEnumerable<LicenseAuditEntry>> GetLicenseAuditHistoryAsync(string licenseId)
```

#### Request/Response Models

**LicenseGenerationRequest**
- Product and consumer identification
- Usage limits (MaxUsers, MaxDevices)
- Feature permissions
- Expiry date and custom properties

**LicenseUpdateRequest**
- Selective property updates
- Maintains cryptographic integrity
- Audit trail preservation

**LicenseValidationResult**
- Validation status and error details
- License object reference
- Validation metadata

### IConsumerAccountService

**Purpose**: Customer account management with relationship tracking.

#### Core Operations

**Account Management**
```csharp
Task<ConsumerAccount> CreateConsumerAccountAsync(ConsumerAccount consumerAccount, string createdBy)
Task<ConsumerAccount> UpdateConsumerAccountAsync(ConsumerAccount consumerAccount, string updatedBy)
Task<bool> DeleteConsumerAccountAsync(string consumerId, string deletedBy)
```

**Status Management**
```csharp
Task<bool> ActivateConsumerAccountAsync(string consumerId, string activatedBy)
Task<bool> DeactivateConsumerAccountAsync(string consumerId, string deactivatedBy, string reason)
Task<bool> UpdateConsumerStatusAsync(string consumerId, ConsumerStatus status, string updatedBy)
```

**Query Operations**
```csharp
Task<ConsumerAccount?> GetConsumerAccountByIdAsync(string consumerId)
Task<ConsumerAccount?> GetConsumerAccountByEmailAsync(string email)
Task<IEnumerable<ConsumerAccount>> GetConsumerAccountsAsync(ConsumerStatus? status = null, bool? isActive = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
```

**Relationship Queries**
```csharp
Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAsync(string accountManagerId, ConsumerStatus? status = null, bool? isActive = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<ConsumerAccount>> GetConsumersByProductAsync(string productId, ConsumerStatus? status = null, bool? isActive = null, string? licenseStatus = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerAndProductAsync(string accountManagerId, string productId, ConsumerStatus? status = null, bool? isActive = null, string? licenseStatus = null, int pageNumber = 1, int pageSize = 50)
```

**Gap Analysis**
```csharp
Task<IEnumerable<ConsumerAccount>> GetConsumersWithoutProductAsync(string productId, ConsumerStatus? status = null, bool? isActive = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<ConsumerAccount>> GetConsumersByAccountManagerWithoutProductAsync(string accountManagerId, string productId, ConsumerStatus? status = null, bool? isActive = null, int pageNumber = 1, int pageSize = 50)
```

### IEnterpriseProductService

**Purpose**: Product lifecycle and hierarchy management.

#### Core Operations

**Product Management**
```csharp
Task<EnterpriseProduct> CreateProductAsync(EnterpriseProduct product, string createdBy)
Task<EnterpriseProduct> UpdateProductAsync(EnterpriseProduct product, string updatedBy)
Task<bool> DeleteProductAsync(string productId, string deletedBy)
```

**Lifecycle Management**
```csharp
Task<bool> UpdateProductStatusAsync(string productId, ProductStatus status, string updatedBy)
Task<bool> DecommissionProductAsync(string productId, DateTime decommissionDate, string decommissionedBy)
```

**Query Operations**
```csharp
Task<EnterpriseProduct?> GetProductByIdAsync(string productId)
Task<EnterpriseProduct?> GetProductByNameAsync(string productName)
Task<IEnumerable<EnterpriseProduct>> GetProductsAsync(ProductStatus? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<EnterpriseProduct>> GetActiveProductsAsync()
Task<IEnumerable<EnterpriseProduct>> GetDeprecatedProductsAsync()
Task<IEnumerable<EnterpriseProduct>> GetProductsNearingDecommissionAsync(int daysFromNow = 30)
```

## Supporting Services

### IProductTierService

**Purpose**: Product tier management within product hierarchy.

#### Operations
- Tier CRUD operations
- Product-tier relationship management
- Tier validation and business rules
- Name uniqueness enforcement

### IProductFeatureService

**Purpose**: Feature management within product tiers.

#### Operations
- Feature CRUD operations
- Feature type classification
- Usage statistics and analytics
- Feature copying between tiers

## Infrastructure Services

### ICryptographicService

**Purpose**: Centralized cryptographic operations for license security.

⚠️ **RESTRICTED ACCESS**: This service should ONLY be used by `IProductLicenseService`. Direct usage by other services is prohibited.

#### Core Operations

**License Key Management**
```csharp
Task<string> GenerateLicenseKeyAsync(string productId, string consumerId, Dictionary<string, object>? additionalData = null)
Task<bool> ValidateLicenseKeyAsync(string licenseKey, string productId, string consumerId)
```

**Digital Signatures**
```csharp
Task<string> GenerateDigitalSignatureAsync(string licenseData)
Task<bool> VerifyDigitalSignatureAsync(string licenseData, string signature)
```

**Encryption & Hashing**
```csharp
Task<string> EncryptDataAsync(string data)
Task<string> DecryptDataAsync(string encryptedData)
Task<string> GenerateLicenseHashAsync(string licenseData)
```

**Activation & Security**
```csharp
Task<string> GenerateActivationCodeAsync(string licenseKey, string machineId)
Task<bool> ValidateActivationCodeAsync(string activationCode, string licenseKey, string machineId)
Task<string> GenerateMachineFingerprintAsync(Dictionary<string, object> machineInfo)
```

**Key Management**
```csharp
Task<bool> RotateKeysAsync()
Task<string> GetCurrentKeyVersionAsync()
```

### IAuditService

**Purpose**: Comprehensive audit trail management for compliance.

#### Operations

**Audit Logging**
```csharp
Task<string> LogAuditEntryAsync(AuditEntry entry)
Task<string> LogLicenseCreatedAsync(ProductLicense license, string createdBy)
Task<string> LogLicenseModifiedAsync(ProductLicense license, string modifiedBy, Dictionary<string, object> changes)
Task<string> LogLicenseStatusChangedAsync(string licenseId, LicenseStatus oldStatus, LicenseStatus newStatus, string changedBy, string? reason = null)
```

**Audit Queries**
```csharp
Task<IEnumerable<AuditEntry>> GetLicenseAuditEntriesAsync(string licenseId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<AuditEntry>> GetConsumerAuditEntriesAsync(string consumerId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
Task<IEnumerable<AuditEntry>> GetUserAuditEntriesAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
```

**Audit Analytics**
```csharp
Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
Task<byte[]> ExportAuditEntriesAsync(string format, string? entityType = null, string? entityId = null, DateTime? fromDate = null, DateTime? toDate = null)
```

## User Experience Services

### INotificationService

**Purpose**: Template-based notification system with delivery tracking.

#### Core Operations

**Notification Delivery**
```csharp
Task<bool> SendLicenseExpirationNotificationAsync(ProductLicense license, int daysUntilExpiry)
Task<bool> SendLicenseActivationNotificationAsync(ProductLicense license, Dictionary<string, object> activationInfo)
Task<bool> SendLicenseRevocationNotificationAsync(ProductLicense license, string reason)
Task<bool> SendCustomNotificationAsync(IEnumerable<string> recipients, string subject, string message, NotificationType notificationType, Dictionary<string, object>? metadata = null)
```

**Template Management**
```csharp
Task<IEnumerable<NotificationTemplate>> GetNotificationTemplatesAsync(NotificationType? notificationType = null)
Task<NotificationTemplate> SaveNotificationTemplateAsync(NotificationTemplate template, string createdBy)
```

**Preference Management**
```csharp
Task<NotificationPreferences> GetNotificationPreferencesAsync(string consumerId)
Task<NotificationPreferences> UpdateNotificationPreferencesAsync(string consumerId, NotificationPreferences preferences)
```

**History & Analytics**
```csharp
Task<IEnumerable<NotificationHistory>> GetLicenseNotificationHistoryAsync(string licenseId, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50)
Task<NotificationStatistics> GetNotificationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
```

### IReportingService

**Purpose**: Advanced reporting and analytics with multiple export formats.

#### Report Generation

**Standard Reports**
```csharp
Task<LicenseUsageReport> GenerateLicenseUsageReportAsync(DateTime fromDate, DateTime toDate, string? productId = null, string? consumerId = null)
Task<LicenseExpirationReport> GenerateLicenseExpirationReportAsync(int daysAhead = 90, string? productId = null)
Task<RevenueReport> GenerateRevenueReportAsync(DateTime fromDate, DateTime toDate, string? productId = null, ReportGroupBy groupBy = ReportGroupBy.Month)
Task<ProductPerformanceReport> GenerateProductPerformanceReportAsync(DateTime fromDate, DateTime toDate)
Task<ConsumerActivityReport> GenerateConsumerActivityReportAsync(DateTime fromDate, DateTime toDate, string? consumerId = null)
```

**Analytics & Dashboards**
```csharp
Task<DashboardAnalytics> GenerateDashboardAnalyticsAsync(DateRange dateRange = DateRange.Last30Days)
Task<CustomReportData> GenerateCustomReportAsync(CustomReportParameters reportParameters)
```

**Export & Scheduling**
```csharp
Task<byte[]> ExportReportAsync(object reportData, ExportFormat format)
Task<string> ScheduleReportAsync(ReportSchedule reportSchedule)
Task<IEnumerable<ReportSchedule>> GetScheduledReportsAsync(bool? isActive = null)
Task<IEnumerable<ReportExecution>> GetReportExecutionHistoryAsync(string? reportType = null, DateTime? fromDate = null, DateTime? toDate = null)
```

## Common Service Patterns

### Validation Pattern
All services implement consistent validation:
```csharp
Task<ValidationResult> ValidateEntityAsync(TEntity entity)
```

### Pagination Pattern
Query operations support pagination:
```csharp
Task<IEnumerable<T>> GetEntitiesAsync(..., int pageNumber = 1, int pageSize = 50)
Task<int> GetEntityCountAsync(...)
```

### Audit Pattern
Critical operations include user tracking:
```csharp
Task<T> OperationAsync(..., string performedBy)
```

### Status Management Pattern
Entities with status support dedicated status operations:
```csharp
Task<bool> UpdateEntityStatusAsync(string entityId, TStatus status, string updatedBy)
```

## Error Handling

### Service-Level Exceptions
- **ValidationException** - Business rule violations
- **NotFoundException** - Entity not found
- **UnauthorizedException** - Access denied
- **CryptographicException** - Security operation failures
- **BusinessLogicException** - Domain rule violations

### Validation Results
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; }
    public IEnumerable<string> Warnings { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

## Performance Considerations

### Async Operations
- All service operations are asynchronous
- Support for cancellation tokens
- Proper async/await usage patterns

### Pagination
- Default page sizes to prevent large data loads
- Count operations separate from data retrieval
- Efficient query patterns

### Caching Strategy
- Service interfaces support caching implementations
- Cache invalidation patterns for data consistency
- Read-heavy operation optimization

## Security Guidelines

### Authentication & Authorization
- All service operations assume authenticated context
- User identification for audit trails
- Role-based access control support

### Data Protection
- Sensitive data validation at service boundaries
- Cryptographic operations centralized
- Audit trail for all data modifications

### Input Validation
- Comprehensive validation at service entry points
- SQL injection prevention through parameterized queries
- XSS prevention in user-generated content

---

**Last Updated**: July 25, 2025  
**Version**: 1.0.0
